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

        [Required(ErrorMessage = "Scrieti mesajul!")]
        //[Display(Name = "Continut")]
        public string Content { get; set; }

        bool Read { get; set; }

        [Required]
        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        //public IEnumerable<Conversation> Conversations { get; set; }

        [Required]
        public string FromUserId { get; set; }

        [Required]
        public string ToUserId { get; set; }
    }
}