/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;

using ASC.Common;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Engine
{
    [Scope]
    public class ParticipantEngine
    {
        private ParticipantHelper ParticipantHelper { get; set; }
        private IDaoFactory DaoFactory { get; set; }

        public ParticipantEngine(IDaoFactory daoFactory, ParticipantHelper participant)
        {
            ParticipantHelper = participant;
            DaoFactory = daoFactory;
        }

        public Participant GetByID(Guid userID)
        {
            var participant = new Participant(userID);
            return ParticipantHelper.InitParticipant(participant);
        }

        public void AddToFollowingProjects(int project, Guid participant)
        {
            DaoFactory.GetParticipantDao().AddToFollowingProjects(project, participant);
        }

        public void RemoveFromFollowingProjects(int project, Guid participant)
        {
            DaoFactory.GetParticipantDao().RemoveFromFollowingProjects(project, participant);
        }

        public List<int> GetInterestedProjects(Guid participant)
        {
            return DaoFactory.GetParticipantDao().GetInterestedProjects(participant);
        }
        public List<int> GetFollowingProjects(Guid participant)
        {
            return new List<int>(DaoFactory.GetParticipantDao().GetFollowingProjects(participant));
        }

        public List<int> GetMyProjects(Guid participant)
        {
            return new List<int>(DaoFactory.GetParticipantDao().GetMyProjects(participant));
        }
    }
}