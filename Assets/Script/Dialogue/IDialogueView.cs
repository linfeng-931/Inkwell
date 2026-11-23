public interface IDialogueView
{
    bool IsTyping { get; }
    float StartTypingTime { get; }
    void UpdateDialogueText(string text);
    void CompleteText();
}