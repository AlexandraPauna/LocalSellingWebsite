using Licenta.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Models
{
    public class ConversationViewModel
    {
        //public virtual IList<ConversationMessage> Conversations { get; set; }
        public virtual IList<ConversationMessage> Conversations { get; set; }
    }

    public class ConversationMessage
    {
        public virtual Conversation Conversation { get; set; }
        public virtual Message LatestMessage { get; set; }
    }
}
