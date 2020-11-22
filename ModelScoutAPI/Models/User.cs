using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelScoutAPI.Models {
    public class User {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public long ChatId { get; set; }
        public int CurrentStep { get; set; }
        public long LastMessageId { get; set; }

        public List<VkAcc> VkAccs { get; } = new List<VkAcc> ();
    }
}