using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Entities
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        [Display(Name = "Anuntul")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        //
        [Required]
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
