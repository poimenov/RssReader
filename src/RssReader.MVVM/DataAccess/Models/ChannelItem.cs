using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.MVVM.DataAccess.Models;

public class ChannelItem
{
    [Key]
    [ReadOnly(true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Browsable(false)]
    public long Id { get; set; }

    [ForeignKey(nameof(ChannelId))]
    [Browsable(false)]
    public int ChannelId { get; set; }

    [Required]
    [DisplayName("Channel")]
    public Channel Channel { get; set; }

    [Required]
    public string ItemId { get; set; }

    [Required]
    [DisplayName("Title")]
    public string Title { get; set; }

    [DisplayName("Link")]
    public string? Link { get; set; }

    [DisplayName("Description")]
    public string? Description { get; set; }

    [DisplayName("Content")]
    public string? Content { get; set; }

    public DateTime? PublishingDate { get; set; }

    [DefaultValue(false)]
    public bool IsRead { get; set; } = false;

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;

    [DefaultValue(false)]
    public bool IsFavorite { get; set; } = false;

    [DefaultValue(false)]
    public bool IsReadLater { get; set; } = false;

    [Browsable(false)]
    public virtual List<ItemCategory> ItemCategories { get; set; }
}
