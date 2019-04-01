using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Licenta.Models.Communication
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Scrieti mesajul!")]
        //[Display(Name = "Continut")]
        public string Content { get; set; }

        public bool Read { get; set; }

        [Required]
        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        //public IEnumerable<Conversation> Conversations { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }
    }
}