﻿using System;

using ASC.Api.Core;

namespace ASC.Projects.Model.Projects
{
    public class ModelAddMilestone
    {
        public string Title { get; set; } 
        public ApiDateTime Deadline { get; set; } 
        public bool IsKey { get; set; } 
        public bool IsNotify { get; set; }
        public string Description { get; set; }
        public Guid Responsible { get; set; }
        public bool NotifyResponsible { get; set; }
    }
}