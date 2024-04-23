using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.MVVM.DataAccess.Models;

public class ChannelsGroup
{
    [Key]
    [ReadOnly(true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Browsable(false)]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [DisplayName("ChannelsGroupName")]
    public string Name { get; set; }

    [Browsable(false)]
    public int Rank { get; set; }

    [Browsable(false)]
    public virtual List<Channel> Channels { get; set; }
}
