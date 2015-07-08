using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var userName = string.Empty;
            var passWord = string.Empty;
            var trelloBoardId = string.Empty;
            var maxTasksString = string.Empty;
            int maxTasks = 10;


            ReadInputFromUser(ref userName, "Please enter your Clarizen Username");
            ReadInputFromUserSecretly(ref passWord, "Please enter your Clarizen Password");
            ReadInputFromUser(ref trelloBoardId, "Please enter the Trello Board ID");
            ReadInputFromUser(ref maxTasksString, "Maxmium Tasks to try and collect from Clarizen");

            try
            {
                maxTasks = Convert.ToInt32(maxTasksString);
            }
            catch (FormatException e)
            {
                Console.WriteLine("Input string is not a sequence of digits.");
            }
            catch (OverflowException e)
            {
                Console.WriteLine("The number cannot fit in an Int32.");
            }


            Bradaz.Clarizen.API.RestClient client = new Bradaz.Clarizen.API.RestClient(userName, passWord);

            client.Data.Query.Select("Name,C_TrelloAPIKey,C_TrelloToken")
                .From("Organization")
                .Limit(1, 0);
            client.ExecuteQuery();

            string id = client.GetCustomApplicationAPIKey("Trello");
            string token = client.GetCustomApplicationToken("Trello");

            ITrello trello = new Trello(id);
            var url = trello.GetAuthorizationUrl("Clarizen2Trello", Scope.ReadWrite, Expiration.Never);
            Console.WriteLine(url);
            
            trello.Authorize(token);

            client.Data.Query.Select("Name,Work,CreatedOn,ActualCost,RemainingEffort,DueDate")
                 .From("Task")
                 .Where("StartDate > 2015-06-01")
                 .Limit(maxTasks,0);
            client.ExecuteQuery();


            var boarder = trello.Boards.WithId(trelloBoardId);


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

            int counting = 0;

            foreach (Bradaz.Clarizen.API.Models.Task t in client.Tasks)
            {

                if (t.RemainingEffort.Value == t.Work.Value)
                {
                    Card newCardTodo = trello.Cards.Add(new NewCard(t.Name, todo));
                    trello.Cards.ChangeDueDate(newCardTodo, t.DueDate);
                    trello.Cards.AddComment(newCardTodo, "Remaining Effort is : " + t.RemainingEffort.Value + " Work Value: " + t.Work.Value);
                    trello.Cards.ChangeDescription(newCardTodo, t.Id);
                    counting++;
                    Console.WriteLine(counting + ". Task Number " + t.SYSID + " " + t.Name);
                }

                else if (t.RemainingEffort.Value == 0)
                {
                    Card newCardDone = trello.Cards.Add(new NewCard(t.Name, done));
                    trello.Cards.ChangeDueDate(newCardDone, t.DueDate);
                    trello.Cards.AddComment(newCardDone, "Remaining Effort is : " + t.RemainingEffort.Value + " Work Value: " + t.Work.Value);
                    trello.Cards.ChangeDescription(newCardDone, t.Id);
                    counting++;
                    Console.WriteLine(counting + ". Task Number " + t.SYSID + " " + t.Name);
                }
                else
                {
                    Card newCardDoing = trello.Cards.Add(new NewCard(t.Name, doing));
                    trello.Cards.ChangeDueDate(newCardDoing, t.DueDate);
                    trello.Cards.AddComment(newCardDoing, "Remaining Effort is : " + t.RemainingEffort.Value + " Work Value: " + t.Work.Value);
                    trello.Cards.ChangeDescription(newCardDoing, t.Id);
                    counting++;
                    Console.WriteLine(counting + ". Task Number " + t.SYSID + " " + t.Name);
                }


            }

            Console.WriteLine();
            Console.Write("Finished.");
            Console.ReadLine();

        }

        //TODO - Need to add these methods to a extenstion helper in Bradaz.Utils.
        static void ReadInputFromUser(ref string input, string displayMessage, string[] validInputStrings = null)
        {
            //string yn = string.Empty;
            bool validated = false;
            string[] ynArray = { "Y", "N", "y", "n" };

            while (true)
            {
                Console.WriteLine(displayMessage);
                input = Console.ReadLine();

                if (validInputStrings != null)
                {
                    if (validInputStrings.Length > 0)
                    {
                        for (int i = 0; i < validInputStrings.Length; i++)
                        {
                            if (validInputStrings[i] == input)
                            {
                                validated = true;
                                Console.WriteLine("You have entered the following " + input);
                                string yn = string.Empty;
                                ReadInputFromUser(ref yn, "Is that OK? ");
                                if (yn == "Y" || yn == "y")
                                {
                                    Debug.WriteLine("Y or N entered " + input);
                                    break;
                                }
                            }
                        }
                    }


                    if (!validated)
                    {
                        Console.WriteLine("What you entered was incorrect " + input + ". You need to input");
                        for (int i = 0; i < validInputStrings.Length; i++)
                        {
                            Console.WriteLine(i + " " + validInputStrings[i]);
                        }

                    }
                    else
                    {
                        break;
                    }
                }

                if (input == "exit" || validInputStrings == null)
                {
                    break;
                }

            }

        }

        static void ReadInputFromUserSecretly(ref string input, string displayMessage)
        {

            Console.WriteLine(displayMessage);
            ConsoleKeyInfo key;
            // Backspace Should Not Work

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    input += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input.Substring(0, (input.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();

        }

    }
}
