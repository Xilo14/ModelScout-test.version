using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelScoutAPI.Models {
    public class VkClient {
        public enum Status {
            Unchecked,
            Accepted,
            Declined,
            Ready,
            Liked,
            Checked,
            Error,
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VkClientId { get; set; }
        public int ProfileVkId { get; set; }
        public Status ClientStatus { get; set; }

        public int VkAccId { get; set; }
        public VkAcc VkAcc { get; set; }
    }
}
