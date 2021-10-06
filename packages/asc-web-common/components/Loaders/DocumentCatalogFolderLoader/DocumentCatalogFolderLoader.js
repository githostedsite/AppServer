import React from 'react';
import PropTypes from 'prop-types';
import {
  StyledContainer,
  StyledBlock,
  StyledRectangleLoader,
} from './StyledDocumentCatalogFolderLoader';
import { inject, observer } from 'mobx-react';

const DocumentCatalogFolderLoader = ({ id, className, style, showText, ...rest }) => {
  return (
    <StyledContainer id={id} className={className} style={style} showText={showText}>
      <StyledBlock>
        <StyledRectangleLoader {...rest} />
        <StyledRectangleLoader {...rest} />
        <StyledRectangleLoader {...rest} />
        <StyledRectangleLoader {...rest} />
      </StyledBlock>
      <StyledBlock>
        <StyledRectangleLoader {...rest} />
        <StyledRectangleLoader {...rest} />
      </StyledBlock>
      <StyledBlock>
        <StyledRectangleLoader {...rest} />
      </StyledBlock>
      <StyledBlock>
        <StyledRectangleLoader {...rest} />
      </StyledBlock>
    </StyledContainer>
  );
};

DocumentCatalogFolderLoader.propTypes = {
  id: PropTypes.string,
  className: PropTypes.string,
  style: PropTypes.object,
  showText: PropTypes.bool,
};

DocumentCatalogFolderLoader.defaultProps = {
  id: undefined,
  className: undefined,
  style: undefined,
};

export default inject(({ auth }) => ({ showText: auth.settingsStore.showText }))(
  observer(DocumentCatalogFolderLoader),
);