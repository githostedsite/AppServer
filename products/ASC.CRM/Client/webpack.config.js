const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const ModuleFederationPlugin = require("webpack").container
  .ModuleFederationPlugin;
const ExternalTemplateRemotesPlugin = require("external-remotes-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const combineUrl = require("@appserver/common/utils/combineUrl");
const AppServerConfig = require("@appserver/common/constants/AppServerConfig");
const sharedDeps = require("@appserver/common/constants/sharedDependencies");

const path = require("path");
const pkg = require("./package.json");
const deps = pkg.dependencies || {};
const homepage = pkg.homepage; // combineUrl(AppServerConfig.proxyURL, pkg.homepage);
const title = pkg.title;

var config = {
  mode: "development",
  entry: "./src/index",

  devServer: {
    devMiddleware: {
      publicPath: homepage,
    },
    static: {
      directory: path.join(__dirname, "dist"),
      publicPath: homepage,
    },

    port: 5014,
    historyApiFallback: {
      // Paths with dots should still use the history fallback.
      // See https://github.com/facebook/create-react-app/issues/387.
      disableDotRule: true,
      index: homepage,
    },
    hot: false,
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
      "Access-Control-Allow-Headers":
        "X-Requested-With, content-type, Authorization",
    },
  },

  output: {
    publicPath: "auto",
    chunkFilename: "static/js/[id].[contenthash].js",
    //assetModuleFilename: "static/images/[hash][ext][query]",
    path: path.resolve(process.cwd(), "dist"),
    filename: "static/js/[name].[contenthash].bundle.js",
  },

  resolve: {
    extensions: [".jsx", ".js", ".json"],
    fallback: {
      crypto: false,
    },
  },

  performance: {
    maxEntrypointSize: 512000,
    maxAssetSize: 512000,
  },

  module: {
    rules: [
      {
        test: /\.(png|jpe?g|gif|ico)$/i,
        type: "asset/resource",
        generator: {
          filename: "static/images/[hash][ext][query]",
        },
      },
      {
        test: /\.m?js/,
        type: "javascript/auto",
        resolve: {
          fullySpecified: false,
        },
      },
      {
        test: /\.react.svg$/,
        use: [
          {
            loader: "@svgr/webpack",
            options: {
              svgoConfig: {
                plugins: [{ removeViewBox: false }],
              },
            },
          },
        ],
      },
      { test: /\.json$/, loader: "json-loader" },
      {
        test: /\.css$/i,
        use: ["style-loader", "css-loader"],
      },
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          "style-loader",
          // Translates CSS into CommonJS
          {
            loader: "css-loader",
            options: {
              url: {
                filter: (url, resourcePath) => {
                  // resourcePath - path to css file

                  // Don't handle `/static` urls
                  if (url.startsWith("/static") || url.startsWith("data:")) {
                    return false;
                  }

                  return true;
                },
              },
            },
          },
          // Compiles Sass to CSS
          "sass-loader",
        ],
      },

      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: [
          {
            loader: "babel-loader",
            options: {
              presets: ["@babel/preset-react", "@babel/preset-env"],
              plugins: [
                "@babel/plugin-transform-runtime",
                "@babel/plugin-proposal-class-properties",
                "@babel/plugin-proposal-export-default-from",
              ],
            },
          },
          "source-map-loader",
        ],
      },
    ],
  },

  plugins: [
    new CleanWebpackPlugin(),
    new ExternalTemplateRemotesPlugin(),
    new CopyPlugin({
      patterns: [
        {
          from: "public",
          globOptions: {
            dot: true,
            gitignore: true,
            ignore: ["**/index.html"],
          },
        },
      ],
    }),
  ],
};

module.exports = (env, argv) => {
  const htmlConfig = {
    template: "./public/index.html",
    publicPath: homepage,
    title: title,
    base: `${homepage}/`,
  };

  const mfConfig = {
      name: "crm",
      filename: "remoteEntry.js",
      remotes: {
        studio: `studio@${combineUrl(
          AppServerConfig.proxyURL,
          "/remoteEntry.js"
        )}`,
      },
      exposes: {
        "./app": "./src/Crm.jsx",
      },
      shared: {
        ...deps,
        ...sharedDeps,
      },
    };

  if (argv.mode === "production") {
    config.mode = "production";
    config.optimization = {
      splitChunks: { chunks: "all" },
      minimize: true,
      minimizer: [new TerserPlugin()],
    };

    console.log("env", env);

    if (env.CDN_URL) {
      const publicPath = combineUrl(env.CDN_URL, homepage);
      console.log("publicPath with env.CDN_URL", publicPath);
      htmlConfig.publicPath = publicPath;
      config.output = { ...config.output, publicPath };
      mfConfig.remotes.studio = `studio@${combineUrl(
        publicPath,
        "/remoteEntry.js"
      )}`;
      console.log("htmlConfig", htmlConfig);
      console.log("mfConfig", mfConfig);
    }
  } else {
    config.devtool = "cheap-module-source-map";
  }

  config.plugins = [
    ...config.plugins,
    new ModuleFederationPlugin(mfConfig),
    new HtmlWebpackPlugin(htmlConfig),
  ];

  return config;
};
