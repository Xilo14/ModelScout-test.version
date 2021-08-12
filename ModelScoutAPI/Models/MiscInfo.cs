using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelScoutAPI.Models {
    public class MiscInfo {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MiscInfoId { get; set; }
        public DateTime LastDateOfClearLimits { get; set; }

    }
}
