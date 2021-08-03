import React from "react";
import { inject, observer } from "mobx-react";
import { withTranslation } from "react-i18next";
import result from "lodash/result";
import find from "lodash/find";
import { withRouter } from "react-router";
import Loaders from "@appserver/common/components/Loaders";
import FilterInput from "@appserver/common/components/FilterInput";
import api from "@appserver/common/api";
import { isMobileOnly } from "react-device-detect";
import moment from "moment";
const { ProjectsFilter, TasksFilter } = api;

const getStatus = (filterValues) => {
  const status = result(
    find(filterValues, (value) => {
      return value.group === "filter-status";
    }),
    "key"
  );
  return status ? status : null;
};

const getMyMilestones = (filterValues) => {
  const myMilestones = result(
    find(filterValues, (value) => {
      return value.group === "filter-my-milestones";
    }),
    "key"
  );
  return myMilestones ? true : null;
};

const getNoMilestones = (filterValues) => {
  const noMilestone = result(
    find(filterValues, (value) => {
      return value.group === "no-milestone";
    }),
    "key"
  );
  return noMilestone ? true : null;
};

const getManager = (filterValues, userId) => {
  const manager = result(
    find(filterValues, (value) => {
      return value.group === "filter-author-manager";
    }),
    "key"
  );
  if (manager === "user-manager-me") {
    return userId;
  }

  return manager ? manager.slice(manager.indexOf("_") + 1) : null;
};

const getCreator = (filterValues) => {
  const creator = result(
    find(filterValues, (value) => {
      return value.group === "filter-my-milestones";
    }),
    "key"
  );
  return creator ? creator : null;
};

const getParticipant = (filterValues, userId) => {
  const participant = result(
    find(filterValues, (value) => {
      return value.group === "filter-author-participant";
    }),
    "key"
  );

  console.log(participant);
  if (participant === "user-team-member-me") {
    return userId;
  }

  if (participant === "no-responsible") {
    return "00000000-0000-0000-0000-000000000000";
  }
  return participant ? participant.slice(participant.indexOf("_") + 1) : null;
};

const getFollow = (filterValues) => {
  const follow = result(
    find(filterValues, (value) => {
      return value.group === "follow";
    }),
    "key"
  );
  return follow ? true : null;
};

const getOverdue = (filterValues) => {
  const overdue = result(
    find(filterValues, (value) => {
      return value.group === "filter-overdue";
    }),
    "key"
  );
  const now = moment();
  return overdue ? now.format() : null;
};

const getMyProject = (filterValues) => {
  const myProject = result(
    find(filterValues, (value) => {
      return value.group === "filter-my-project";
    }),
    "key"
  );
  return myProject ? true : null;
};

const getDepartament = (filterValues) => {
  const departament = result(
    find(filterValues, (value) => {
      return value.group === "filter-author-departament";
    }),
    "key"
  );
  return departament ? departament.slice(departament.indexOf("_") + 1) : null;
};

const getNoTag = (filterValues) => {
  const notag = result(
    find(filterValues, (value) => {
      return value.group === "notag";
    }),
    "key"
  );
  return notag ? true : null;
};

const getSelectedItem = (filterValues, type) => {
  const selectedItem = filterValues.find((item) => item.key === type);
  return selectedItem || null;
};
const PureSectionFilterContent = (props) => {
  const {
    t,
    filter,
    customNames,
    fetchProjects,
    fetchTasks,
    tReady,
    getProjectFilterCommonOptions,
    getTaskFilterCommonOptions,
    getProjectFilterSortDataOptions,
    getTaskFilterSortDataOptions,
    sectionWidth,
    user,
  } = props;

  const translations = {
    status: t("Status"),
    active: t("Active"),
    paused: t("Paused"),
    closed: t("Closed"),
    byProjectManager: t("ByProjectManager"),
    me: t("Me"),
    meLabel: t("Common:MeLabel"),
    select: t("Common:Select"),
    otherUsers: t("OtherUsers"),
    other: t("Other"),
    followed: t("Followed"),
    withTag: t("WithTag"),
    withoutTag: t("WithoutTag"),
    teamMember: t("TeamMember"),
    groups: t("Groups"),

    responsible: t("Responsible"),
    noResponsible: t("noResponsible"),
    milestone: t("Milestone"),
    milestonesWithMyTasks: t("MilestonesWithMyTasks"),
    noMilestone: t("NoMilestone"),
    otherMilestones: t("otherMilestones"),
    allClosed: t("AllClosed"),
    creator: t("Creator"),
    open: t("Open"),
    project: t("Project"),
    myProject: t("MyProject"),
    otherProjects: t("OtherProjects"),
    dueDate: t("DueDate"),
    overdue: t("Overdue"),
    today: t("Today"),
    upcoming: t("Upcoming"),
    customPeriod: t("CustomPeriod"),

    byCreationDate: t("ByCreationDate"),
    byTitle: t("ByTitle"),
    priority: t("Priority"),
    startDate: t("StartDate"),
    order: t("Order"),
  };

  const onProjectFilter = (data) => {
    const status = getStatus(data.filterValues) || null;
    const search = data.inputValue || "";
    const sortBy = data.sortId;
    const sortOrder =
      data.sortDirection === "desc" ? "descending" : "ascending";
    const manager = getManager(data.filterValues, user.id) || null;
    const participant = getParticipant(data.filterValues, user.id) || null;
    const departament = getDepartament(data.filterValues) || null;
    const follow = getFollow(data.filterValues) || null;
    const notag = getNoTag(data.filterValues) || null;

    const selectedItem =
      manager !== "user-manager-me"
        ? getSelectedItem(data.filterValues, manager)
        : participant !== "user-team-member-me"
        ? getSelectedItem(data.filterValues, participant)
        : null;
    const selectedFilterItem = {};

    if (selectedItem) {
      selectedFilterItem.key = selectedItem.selectedItem.key;
      selectedFilterItem.label = selectedItem.selectedItem.label;
      selectedFilterItem.type = selectedItem.typeSelector;
    }

    const newFilter = filter.clone();

    notag ? (newFilter.tag = -1) : (newFilter.tag = 0);

    newFilter.page = 0;
    newFilter.sortBy = sortBy;
    newFilter.sortOrder = sortOrder;
    newFilter.search = search;
    newFilter.selectedItem = selectedFilterItem;
    newFilter.status = status;
    newFilter.participant = participant;
    newFilter.manager = manager;
    newFilter.departament = departament;
    newFilter.follow = follow;

    fetchProjects(newFilter, newFilter.folder);
  };

  const onTaskFilter = (data) => {
    const participant = getParticipant(data.filterValues, user.id) || null;
    const departament = getDepartament(data.filterValues) || null;
    const myMilestones = getMyMilestones(data.filterValues) || null;
    const noMilestone = getNoMilestones(data.filterValues) || null;
    const myProjects = getMyProject(data.filterValues) || null;
    const creator = getManager(data.filterValues, user.id) || null;
    const notag = getNoTag(data.filterValues) || null;
    const status = getStatus(data.filterValues) || null;
    const overdue = getOverdue(data.filterValues) || null;
    const search = data.inputValue || "";
    const sortBy = data.sortId;
    const sortOrder =
      data.sortDirection === "desc" ? "descending" : "ascending";

    const newFilter = filter.clone();
    notag ? (newFilter.tag = -1) : (newFilter.tag = 0);
    newFilter.page = 0;
    newFilter.sortBy = sortBy;
    newFilter.sortOrder = sortOrder;
    newFilter.search = search;
    newFilter.participant = participant;
    newFilter.departament = departament;
    newFilter.myMilestones = myMilestones;
    newFilter.noMilestone = noMilestone;
    newFilter.creator = creator;
    newFilter.status = status;
    newFilter.myProjects = myProjects;
    newFilter.deadlineStop = overdue;

    fetchTasks(newFilter, newFilter.folder);
  };

  const onFilter = (data) => {
    if (filter instanceof ProjectsFilter) {
      onProjectFilter(data);
    }

    if (filter instanceof TasksFilter) {
      onTaskFilter(data);
    }
  };

  const getData = () => {
    const { user } = props;
    const { selectedItem } = filter;
    if (filter instanceof ProjectsFilter) {
      return getProjectFilterCommonOptions(
        translations,
        customNames,
        selectedItem,
        user
      );
    } else if (filter instanceof TasksFilter) {
      return getTaskFilterCommonOptions(
        translations,
        customNames,
        selectedItem,
        user
      );
    }
  };

  const getFilterSortData = () => {
    if (filter instanceof ProjectsFilter) {
      return getProjectFilterSortDataOptions(translations);
    }

    if (filter instanceof TasksFilter) {
      return getTaskFilterSortDataOptions(translations);
    }
  };
  const getSortData = () => {
    const commonOptions = getFilterSortData();

    const viewSettings = [
      { key: "row", label: t("ViewList"), isSetting: true, default: true },
      { key: "tile", label: t("ViewTiles"), isSetting: true, default: true },
    ];

    return window.innerWidth < 460
      ? [...commonOptions, ...viewSettings]
      : commonOptions;
  };
  const filterColumnCount =
    window.innerWidth < 500 ? {} : { filterColumnCount: 3 };

  const getTaskFilterSelectedData = (selectedFilterData) => {
    selectedFilterData.sortId = filter.sortBy;
    return selectedFilterData;
  };

  const getProjectFilterSelectedData = (selectedFilterData, filter) => {
    // selectedFilterData.sortId = filter.sortBy ? filter.sortBy : "create_on";
    // bug with selectedFilterData

    // if (filter.status) {
    //   selectedFilterData.filterValues.push({
    //     key: `${filter.status}`,
    //     group: "filter-status",
    //   });
    // }

    // if (filter.follow) {
    //   selectedFilterData.filterValues.push({
    //     key: "follow",
    //     group: "follow",
    //   });
    // }

    // if (filter.notag) {
    //   selectedFilterData.filterValues.push({
    //     key: "notag",
    //     group: "notag",
    //   });
    // }

    // if (filter.participant) {
    //   selectedFilterData.filterValues.push({
    //     key: `user_${filter.participant}`,
    //     group: "filter-author-participant",
    //   });
    // }

    return selectedFilterData;
  };

  const getSelectedFilterData = () => {
    const selectedFilterData = {
      filterValues: [],
      sortDirection: filter.sortOrder === "ascending" ? "asc" : "desc",
      sortId: filter.sortBy,
    };

    selectedFilterData.inputValue = filter.search;

    if (filter instanceof ProjectsFilter) {
      return getProjectFilterSelectedData(selectedFilterData, filter);
    }

    // if (filter instanceof TasksFilter) {
    //   return getTaskFilterSelectedData(selectedFilterData);
    // }
    // пока что оставить так
    // return selectedFilterData;
  };

  const selectedFilterData = getSelectedFilterData();

  return !tReady ? (
    <Loaders.Filter />
  ) : (
    <FilterInput
      sectionWidth={sectionWidth}
      getFilterData={getData}
      getSortData={getSortData}
      onFilter={onFilter}
      selectedFilterData={selectedFilterData}
      isReady={tReady}
      directionAscLabel={t("Common:DirectionAscLabel")}
      directionDescLabel={t("Common:DirectionDescLabel")}
      isMobile={isMobileOnly}
      placeholder={t("Common:Search")}
      {...filterColumnCount}
      contextMenuHeader={t("Common:AddFilter")}
    />
  );
};

const SectionFilterContent = withTranslation(["Home", "Common"])(
  withRouter(PureSectionFilterContent)
);

export default inject(
  ({ auth, projectsStore, projectsFilterStore, tasksFilterStore }) => {
    const { customNames } = auth.settingsStore;
    const {
      fetchProjects,
      getProjectFilterCommonOptions,
      getProjectFilterSortDataOptions,
    } = projectsFilterStore;
    const { user } = auth.userStore;
    const { filter } = projectsStore;

    const { manager, participant, search } = filter;

    const isFiltered = manager || participant || search;

    const {
      getTaskFilterCommonOptions,
      getTaskFilterSortDataOptions,
      fetchTasks,
    } = tasksFilterStore;
    return {
      customNames,
      filter,
      isFiltered,
      user,
      fetchProjects,
      fetchTasks,
      getProjectFilterCommonOptions,
      getTaskFilterCommonOptions,
      getProjectFilterSortDataOptions,
      getTaskFilterSortDataOptions,
    };
  }
)(observer(SectionFilterContent));