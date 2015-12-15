using OmiyaGames;

public class StartMultiplayerMenu : StartMenu
{
    public override void StartAction()
    {
        Manager.Show<SetupMenu>();
        Manager.ButtonClick.Play();
    }

    public void OnHowToPlayClicked()
    {
        Manager.Show<HowToPlayMenu>();
        Manager.ButtonClick.Play();
    }
}
