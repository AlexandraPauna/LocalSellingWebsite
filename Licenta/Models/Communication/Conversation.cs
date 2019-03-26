using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Licenta.Models.Communication
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        [Display(Name = "Anuntul")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        //[Required]
        //public string FromUser { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

    }
}