using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CheckMeetingBot.DialogBot
{
    public class DialogCheckMeeting : ComponentDialog
    {
        private readonly IStatePropertyAccessor<CheckMeetingMethod> _CheckMeetingAccessor;

        public DialogCheckMeeting(UserState userState)
            : base(nameof(DialogCheckMeeting))
        {
            _CheckMeetingAccessor = userState.CreateProperty<CheckMeetingMethod>("CheckMeeting");

            var waterfallsteps = new WaterfallStep[]
                {
                    Name,
                    Show,
                    End,
                };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallsteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> Name(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var promptOptions = new PromptOptions

            {
                Prompt = MessageFactory.Text("Please enter your name Or 'End' to finish. "),

            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

        }
        private async Task<DialogTurnResult> Show(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            var user = stepContext.Context.Activity.From.Name;

            int countName;

            var CheckMeetingMethod = await _CheckMeetingAccessor.GetAsync(stepContext.Context, () => new CheckMeetingMethod(), cancellationToken);
            var name = (string)stepContext.Values["name"];
            CheckMeetingMethod.Name.Add(user);
            var newLine = Environment.NewLine;

            var listName = CheckMeetingMethod.Name;

            countName = listName.Count();
            var message = "";

            if (name == "End" || name == "end")
            {
                listName.Remove(listName[countName - 1]);
                message = $"Total of members : {countName - 1} {newLine} Members are : {newLine} {string.Join(newLine, listName)}";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(message),
                }, cancellationToken);

            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(DialogCheckMeeting), listName, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int countName;

            var CheckMeetingMethod = await _CheckMeetingAccessor.GetAsync(stepContext.Context, () => new CheckMeetingMethod(), cancellationToken);

            var listName = CheckMeetingMethod.Name;

            countName = listName.Count;

            listName.RemoveRange(0, countName);

            return await stepContext.BeginDialogAsync(nameof(DialogCheckMeeting), listName, cancellationToken);
        }

    }
}
