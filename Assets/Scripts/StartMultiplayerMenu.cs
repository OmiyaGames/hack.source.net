using OmiyaGames;

public class StartMultiplayerMenu : StartMenu
{
    public override void StartAction()
    {
        Manager.Show<SetupMenu>();
    }

    public void OnInstructionsClicked()
    {
        Manager.ButtonClick.Play();
    }
}
