using UnityEngine;

public static class AudioSettingsStore
{
    private const string KeyMusic = "ZH_VOL_MUSIC";
    private const string KeyJump = "ZH_VOL_JUMP";
    private const string KeyRun = "ZH_VOL_RUN";
    private const string KeyDamage = "ZH_VOL_DAMAGE";
    private const string KeyVictory = "ZH_VOL_VICTORY";
    private const string KeyCollectible = "ZH_VOL_COLLECTIBLE";
    private const string KeyPause = "ZH_VOL_PAUSE";

    public static float Music => PlayerPrefs.GetFloat(KeyMusic, 0.35f);
    public static float Jump => PlayerPrefs.GetFloat(KeyJump, 1f);
    public static float Run => PlayerPrefs.GetFloat(KeyRun, 0.75f);
    public static float Damage => PlayerPrefs.GetFloat(KeyDamage, 1f);
    public static float Victory => PlayerPrefs.GetFloat(KeyVictory, 1f);
    public static float Collectible => PlayerPrefs.GetFloat(KeyCollectible, 1f);
    public static float Pause => PlayerPrefs.GetFloat(KeyPause, 1f);

    public static void SetMusic(float value) => Set(KeyMusic, value);
    public static void SetJump(float value) => Set(KeyJump, value);
    public static void SetRun(float value) => Set(KeyRun, value);
    public static void SetDamage(float value) => Set(KeyDamage, value);
    public static void SetVictory(float value) => Set(KeyVictory, value);
    public static void SetCollectible(float value) => Set(KeyCollectible, value);
    public static void SetPause(float value) => Set(KeyPause, value);

    private static void Set(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}
