using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PMDWebApplication.DB;

namespace PMDWebApplication.Models
{
    public class MessageReplyViewModel
    {
        private List<MessageReply> _replies = new List<MessageReply>();
        public Reply Reply { get; set; }

        public Msg Message { get; set; }

        public List<MessageReply> Replies
        {
            get { return _replies; }
            set { _replies = value; }
        }

        public PagedList.IPagedList<AspNetMessage> Messages { get; set; }

        public class MessageReply
        {
            public int Id { get; set; }
            public int MessageId { get; set; }
            public string MessageDetails { get; set; }
            public string ReplyFrom { get; set; }

            public string ReplyMessage { get; set; }
            public DateTime ReplyDateTime { get; set; }
        }
    }
}