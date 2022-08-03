using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Domain.Struct
{
    public  struct RelatedEvents
    {
        public static List<int> documentExeptions = new List<int> {

                 (int) NotifEvent.AddComTeamComment
                , (int) NotifEvent.AddComTeamCommentReply
                , (int) NotifEvent.AddTQTeamComment
                , (int) NotifEvent.AddTQTeamCommentReply
                , (int) NotifEvent.AddNCRTeamComment
                , (int) NotifEvent.AddNCRTeamCommentReply
                , (int) NotifEvent.AddRevisionComment
                , (int) NotifEvent.ReplayRevisionComment
            };

        public static List<int> constructionExeptions = new List<int> {

                (int)NotifEvent.AddCommentInOperation,(int)NotifEvent.CommentReplyInOperation
        };
    }
}
