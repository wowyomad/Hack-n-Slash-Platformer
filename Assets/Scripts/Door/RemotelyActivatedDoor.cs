using TheGame;
public class RemotelyActivatedDoor : Door, IActivatable
{
    public void Activate()
    {
        Open();
    }

    public void Deactivate()
    {   
        Close();
    }
}
