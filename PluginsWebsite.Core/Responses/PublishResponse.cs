using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Responses
{
    public class PublishResponse
    {
        public PublishResponse() { }

        public PublishResponse(string message)
        {
            IsSuccess = false;
            Message = message;
        }

        public PublishResponse(int pluginId)
        {
            IsSuccess = true;
            PluginId = pluginId;
        }

        public PublishResponse(bool success, string message, int pluginId = 0)
        {
            IsSuccess = success;
            Message = message;
            PluginId = pluginId;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int PluginId { get; set; }
    }
}
