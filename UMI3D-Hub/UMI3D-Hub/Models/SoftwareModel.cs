/*
Copyright 2022 Quentin Tran
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;

namespace UMI3DHub.Models
{
    public class SoftwareModel
    {
        public int Id;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepoOwner { get; set; } = string.Empty;
        public string RepoName { get; set; } = string.Empty;

        public List<SoftwareVersionModel> versions = new List<SoftwareVersionModel>();
    }
}
