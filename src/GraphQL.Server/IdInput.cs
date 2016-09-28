using System.ComponentModel.DataAnnotations;

namespace GraphQL.Server
{
    public class IdInput
    {
        [Required]
        public int? Id { get; set; }
    }
}