using Exiled.API.Interfaces;

namespace NPCS
{
    public class Translations: ITranslation
    {
        public string AlreadyTalking { get; set; } = "We are already talking!";
        public string NpcBusy { get; set; } = "I'm busy now, wait a second";
        public string TalkEnd { get; set; } = "ended talk";
        public string InvalidAnswer { get; set; } = "Invalid answer!";
        public string IncorrectFormat { get; set; } = "Incorrect answer format!";
        public string NotTalking { get; set; } = "You aren't talking to this NPC!";
        public string NpcNotFound { get; set; } = "NPC not found!";
        public string AnswerNumber { get; set; } = "You must provide answer number!";
        public string OnlyPlayers { get; set; } = "Only players can use this!";
    }
}
