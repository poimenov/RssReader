using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.MVVM.DataAccess.Models;

public class Channel
{
    [Key]
    [ReadOnly(true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Browsable(false)]
    public int Id { get; set; }

    [ForeignKey(nameof(ChannelsGroupId))]
    [Browsable(false)]
    public int? ChannelsGroupId { get; set; }

    [DisplayName("ChannelsGroup")]
    public ChannelsGroup? ChannelsGroup { get; set; }

    [Browsable(false)]
    public int Rank { get; set; }

    [Required]
    [StringLength(250, MinimumLength = 3)]
    [DisplayName("Title")]
    public string Title { get; set; }

    [DisplayName("Description")]
    public string? Description { get; set; }

    [DisplayName("Link")]
    public string? Link { get; set; }

    [Required]
    [DisplayName("Url")]
    public string Url { get; set; }

    [DisplayName("ImageUrl")]
    public string? ImageUrl { get; set; }

    [DisplayName("Language")]
    public string? Language { get; set; }

    [DisplayName("Language")]
    public DateTime? LastUpdatedDate { get; set; }

    [Browsable(false)]
    public virtual List<ChannelItem> Items { get; set; }
}
