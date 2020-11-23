using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelScoutAPI.Models
{
    public class VkAcc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VkAccId { get; set; }
        public string AccessToken { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int FriendsLimit { get; set; }
        public int CountAddedFriends { get; set; }

        //SearchConfig
        public int BirthMonth { get; set; }
        public int BirthDay { get; set; }
        public int AgeTo { get; set; }
        public int AgeFrom { get; set; }
        public int City { get; set; }
        public int Country { get; set; }
        public int Sex { get; set; }
        //SearchConfig

        public int UserId { get; set; }
        public User User { get; set; }

        public List<VkClient> VkClients { get; } = new List<VkClient>();

    }
}