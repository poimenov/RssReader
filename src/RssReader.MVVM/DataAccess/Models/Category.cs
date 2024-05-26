using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.MVVM.DataAccess.Models;

public class Category
{
    [Key]
    [ReadOnly(true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Browsable(false)]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [DisplayName("CategoryName")]
    public string Name { get; set; }

    [Browsable(false)]
    public virtual List<ItemCategory> ItemCategories { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
