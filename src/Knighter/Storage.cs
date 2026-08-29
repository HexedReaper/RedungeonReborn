using Android.Content;
using Android.Preferences;
using Microsoft.Xna.Framework;

namespace Knighter;

public class Storage : Component, IStorage
{
	private ISharedPreferences preferences;

	private ISharedPreferencesEditor editor;

	public Storage()
	{
		preferences = PreferenceManager.GetDefaultSharedPreferences(Game.Activity.ApplicationContext);
		editor = preferences.Edit();
	}

	public void Save()
	{
		editor.Apply();
	}

	public void SetField(string key, string value)
	{
		editor.PutString(key, value);
	}

	public string GetField(string key)
	{
		return preferences.GetString(key, string.Empty);
	}

	public bool FieldExist(string key)
	{
		return preferences.Contains(key);
	}

	public bool GetBool(string key)
	{
		if (!bool.TryParse(GetField(key), out var result))
		{
			return false;
		}
		return result;
	}

	public int GetInt(string key)
	{
		if (!int.TryParse(GetField(key), out var result))
		{
			return 0;
		}
		return result;
	}

	public void SetBool(string key, bool value)
	{
		SetField(key, value.ToString());
	}

	public void SetInt(string key, int value)
	{
		SetField(key, value.ToString());
	}

	public void SetString(string key, string value)
	{
		SetField(key, value);
	}

	public bool TryGetBool(string key, ref bool result)
	{
		if (FieldExist(key))
		{
			result = GetBool(key);
			return true;
		}
		return false;
	}

	public bool TryGetInt(string key, ref int result)
	{
		if (FieldExist(key))
		{
			result = GetInt(key);
			return true;
		}
		return false;
	}

	public bool TryGetString(string key, ref string result)
	{
		if (FieldExist(key))
		{
			result = GetField(key);
			return true;
		}
		return false;
	}
}
