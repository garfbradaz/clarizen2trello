using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloNet;

namespace Clarizen2Trello
{
    class Program
    {
        static void Main(string[] args)
        {
            ///Put your    username and password here.
            Bradaz.Clarizen.API.RestClient client = new Bradaz.Clarizen.API.RestClient("garfbradaz","Poohead26@");


            client.Data.Query.Select("Name,C_TrelloAPIKey")
                .From("Organization")
                .Limit(1, 0);
            client.ExecuteQuery();

            string id = client.GetCustomApplication("Trello");
            ITrello trello = new Trello(id);
            var url = trello.GetAuthorizationUrl("Clarizen2Trello", Scope.ReadWrite, Expiration.Never);
            Console.WriteLine(url);
            trello.Authorize("feaaef78396cb07dd29f6add6605840ebde8be229ef811ea60d86cbe1b0eb43f");


            client.Data.Query.Select("Name,Work,CreatedOn,ActualCost,RemainingEffort,DueDate") 
                 .From("Task")
                 .Where("StartDate > 2015-06-01");
            client.ExecuteQuery();

            var boarder = trello.Boards.WithId("558720ad02baab850f911446");


            IEnumerable<TrelloNet.List> lists = trello.Lists.ForBoard(boarder, ListFilter.Open);
            List todo = new List();
            List doing = new List();
            List done = new List();

            foreach (List l in lists)
            {
                if (l.Name == "To Do")
                {
                    todo = l;
                }
                if(l.Name == "Doing")
                {
                    doing = l;
                }
                if(l.Name == "Done")
                {
                    done = l;
                }
            }

            foreach (Bradaz.Clarizen.API.Models.Task t in client.Tasks)
            {

                if (t.RemainingEffort.Value == t.Work.Value)
                {
                    Card newCardTodo = trello.Cards.Add(new NewCard(t.Name, todo));
                    trello.Cards.ChangeDueDate(newCardTodo, t.DueDate);
                    trello.Cards.AddComment(newCardTodo, "Remaining Effort is : " + t.RemainingEffort.Value);
                    trello.Cards.ChangeDescription(newCardTodo, t.Id);
                }

                if (t.RemainingEffort.Value == 0)
                {
                    Card newCardDone = trello.Cards.Add(new NewCard(t.Name, done));
                    trello.Cards.ChangeDueDate(newCardDone, t.DueDate);
                    trello.Cards.AddComment(newCardDone, "Remaining Effort is : " + t.RemainingEffort.Value);
                    trello.Cards.ChangeDescription(newCardDone, t.Id);
                }
                else
                {
                    Card newCardDoing = trello.Cards.Add(new NewCard(t.Name, doing));
                    trello.Cards.ChangeDueDate(newCardDoing, t.DueDate);
                    trello.Cards.AddComment(newCardDoing, "Remaining Effort is : " + t.RemainingEffort.Value);
                    trello.Cards.ChangeDescription(newCardDoing, t.Id);
                }




            }

            ///var board = trello.Boards.WithId("55394100a6c3ca74cb115269");


           // Console.WriteLine(board.Name);

            Console.ReadLine();
        }
    }
}
