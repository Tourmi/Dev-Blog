using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class TagSearchResults
    {

        public class Result
        {
            public Result()
            {
                Children = new List<Result>();
            }

            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("children")]
            public List<Result> Children { get; set; }

            public bool ShouldSerializeChildren() => Children.Any();
            public bool ShouldSerializeId() => Id != null;
        }


        public TagSearchResults()
        {
            results = new List<Result>();
        }

        private List<Result> results;

        [JsonProperty("results")]
        public IReadOnlyList<Result> Results { get { return results; } }

        public void AddResult(string id, string text) => results.Add(new Result() { Id = id, Text = text });
        public void AddGroup(Result group) => results.Add(group);
    }
}
