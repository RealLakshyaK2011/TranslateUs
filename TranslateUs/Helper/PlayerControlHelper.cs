namespace TranslateUs.Helper;

public class PlayerControlHelper
{
    public static PlayerControl GetPlayerControlById(byte playerId)
    {
        foreach (PlayerControl plr in PlayerControl.AllPlayerControls)
        {
            if (plr.PlayerId == playerId)
            {
                return plr;
            }
        }

        return null;
    }
}