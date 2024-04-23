using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.MVVM.DataAccess.Models;

public class ItemCategory
{
    [Key]
    [ReadOnly(true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Browsable(false)]
    public long Id { get; set; }

    [ForeignKey(nameof(ChannelItemId))]
    [Browsable(false)]
    public long ChannelItemId { get; set; }

    [Required]
    [DisplayName("ChannelItem")]
    public ChannelItem ChannelItem { get; set; }

    [ForeignKey(nameof(CategoryId))]
    [Browsable(false)]
    public int CategoryId { get; set; }

    [Required]
    [DisplayName("Category")]
    public Category Category { get; set; }
}
