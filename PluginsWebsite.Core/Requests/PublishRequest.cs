using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Requests
{
    public class PublishRequest
    {
        public PublishRequest() { }

        public PublishRequest(string owner, string repositoryName) 
        {
            RepositoryName = repositoryName;
            RepositoryOwner = owner;
        }

        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}
