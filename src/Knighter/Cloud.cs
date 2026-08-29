using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Games;
using Android.Gms.Games.Snapshot;
using Knighter.Entities;
using Knighter.Helpers;

namespace Knighter;

public class Cloud : Component
{
	private const string SavedGameFileName = "saved_game";

	private const int STATUS_OK = 0;

	private const int STATUS_SNAPSHOT_CONFLICT = 4004;

	private const int STATUS_SNAPSHOT_CONTENTS_UNAVAILABLE = 4002;

	private ISnapshot snapshot;

	private Dictionary<string, string> fields;

	private bool downloading;

	private bool uploading;

	private int downloads;

	private GoogleApiClient client => base.core.Game.GooglePlayHelper.GetGoogleApiClient();

	public event EventHandler DidDownloaded;

	public event EventHandler DidUploaded;

	public void PlatformInitialize()
	{
	}

	public Dictionary<string, string> PlatformDownload()
	{
		snapshot = null;
		if (!DownloadSnapshot())
		{
			return null;
		}
		return LoadDataFromSnapshot();
	}

	public void PlatformUpload(Dictionary<string, string> d)
	{
		SaveDataIntoSnapshot(d);
		UploadSnapshot();
		snapshot = null;
	}

	public bool PlatformIsCloudAvailable()
	{
		return base.core.Game.GooglePlayHelper.IsConnected;
	}

	private bool DownloadSnapshot()
	{
		try
		{
			ISnapshotsOpenSnapshotResult result = GamesClass.Snapshots.Open(client, "saved_game", createIfNotFound: true).AsAsync<ISnapshotsOpenSnapshotResult>().Result;
			int num = 2;
			while (num-- > 0)
			{
				switch (result.Status.StatusCode)
				{
				case 0:
					this.snapshot = result.Snapshot;
					return true;
				case 4002:
					this.snapshot = result.Snapshot;
					return true;
				case 4004:
				{
					string conflictId = result.ConflictId;
					ISnapshot snapshot = result.Snapshot;
					_ = result.ConflictingSnapshot;
					result = GamesClass.Snapshots.ResolveConflict(client, conflictId, snapshot).AsAsync<ISnapshotsOpenSnapshotResult>().Result;
					break;
				}
				}
			}
		}
		catch (Exception ex)
		{
			Exception("Cloud.DownloadSnapshotA: " + ex.Message, isFatal: false);
		}
		return false;
	}

	private bool UploadSnapshot()
	{
		try
		{
			ISnapshotMetadataChange metadataChange = new SnapshotMetadataChangeBuilder().SetDescription("Redungeon saved game").Build();
			if (GamesClass.Snapshots.CommitAndClose(client, snapshot, metadataChange).AsAsync<ISnapshotsCommitSnapshotResult>().Result.Status.StatusCode == 0)
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			Exception("Cloud.UploadSnapshotA: " + ex.Message, isFatal: false);
		}
		return false;
	}

	private Dictionary<string, string> LoadDataFromSnapshot()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (snapshot == null)
		{
			return dictionary;
		}
		if (snapshot.SnapshotContents == null)
		{
			return dictionary;
		}
		byte[] bytes = snapshot.SnapshotContents.ReadFully();
		string[] array = Encoding.UTF8.GetString(bytes).Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			dictionary[array2[0]] = array2[1];
		}
		return dictionary;
	}

	private void SaveDataIntoSnapshot(Dictionary<string, string> d)
	{
		if (snapshot != null)
		{
			string s = string.Join(";", d.Select((KeyValuePair<string, string> x) => x.Key + "=" + x.Value).ToArray());
			byte[] bytes = new UTF8Encoding().GetBytes(s);
			snapshot.SnapshotContents.WriteBytes(bytes);
		}
	}

	public Cloud()
	{
		fields = new Dictionary<string, string>();
		PlatformInitialize();
	}

	private void MergeData()
	{
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			bool num = GetBool($"character-{value}-unlocked");
			int num2 = GetInt($"character-{value}-level");
			if (num)
			{
				base.core.ProfileData.Characters[value].Unlocked = true;
				if (base.core.ProfileData.Characters[value].Level < num2)
				{
					base.core.ProfileData.Characters[value].Level = num2;
				}
			}
		}
		int num3 = GetInt("best-distance");
		if (base.core.ProfileData.BestDistance < num3)
		{
			base.core.ProfileData.BestDistance = num3;
		}
		if (base.core.ProfileData.InitiallyMerged)
		{
			base.core.ProfileData.Coins = Math.Max(base.core.ProfileData.Coins, GetInt("coins"));
		}
		else
		{
			base.core.ProfileData.AddCoins(GetInt("coins"));
		}
		foreach (Achievement value2 in Enum.GetValues(typeof(Achievement)))
		{
			if (GetBool($"achievment-{value2}") && !base.core.ProfileData.IsAchievementUnlocked(value2))
			{
				base.core.ProfileData.UnlockAchievement(value2, saveImmediately: false);
			}
		}
		foreach (Stat value3 in Enum.GetValues(typeof(Stat)))
		{
			if (value3 != Stat.Sessions)
			{
				int num4 = GetInt($"stat-{value3}");
				if (base.core.ProfileData.InitiallyMerged)
				{
					base.core.ProfileData.SetStat(value3, Math.Max(num4, _stat(value3)));
				}
				else
				{
					base.core.ProfileData.IncStat(value3, num4);
				}
			}
		}
		base.core.ProfileData.AdsRemoved |= GetBool("remove-ads");
		base.core.ProfileData.CoinDoublerEnabled |= GetBool("coin-doubler-enabled");
		base.core.ProfileData.InitiallyMerged = true;
		base.core.ProfileData.SaveIntoStorage();
	}

	private int CalculateRefund()
	{
		int num = 0;
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			CharDescription charDescription = CharDescription.Get[value];
			bool flag = GetBool($"character-{value}-unlocked");
			bool unlocked = base.core.ProfileData.Characters[value].Unlocked;
			bool flag2 = base.core.ProfileData.DeltaSyncData.Unlocks[value];
			if (!unlocked)
			{
				continue;
			}
			if ((unlocked && !flag2) & flag)
			{
				num += charDescription.UnlockPrice;
			}
			int num2 = GetInt($"character-{value}-level");
			int level = base.core.ProfileData.Characters[value].Level;
			int num3 = base.core.ProfileData.DeltaSyncData.Levels[value];
			if (num2 > 1 && level != num3)
			{
				for (int i = num3; i < Math.Min(level, num2); i++)
				{
					num += charDescription.Levels[i].Price;
				}
			}
		}
		return num;
	}

	private bool HasDataFromAnyDevice()
	{
		if (GetField("device-id").Equals(string.Empty))
		{
			return false;
		}
		string field = GetField("version");
		if (field.Equals(string.Empty))
		{
			return false;
		}
		string versionString = base.core.SystemCalls.GetVersionString(withBuildNumber: false);
		if (field.CompareTo(versionString) > 0)
		{
			return false;
		}
		return true;
	}

	private bool HasDataFromOtherDevice()
	{
		if (!HasDataFromAnyDevice())
		{
			return false;
		}
		string field = GetField("device-id");
		string deviceUniqueId = base.core.SystemCalls.GetDeviceUniqueId();
		return field != deviceUniqueId;
	}

	private Dictionary<string, string> BuildDataForUploading()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["device-name"] = base.core.SystemCalls.GetDeviceName();
		dictionary["device-id"] = base.core.SystemCalls.GetDeviceUniqueId();
		dictionary["version"] = base.core.SystemCalls.GetVersionString(withBuildNumber: false);
		dictionary["best-distance"] = base.core.ProfileData.BestDistance.ToString();
		dictionary["coins"] = base.core.ProfileData.Coins.ToString();
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			dictionary[$"character-{value}-unlocked"] = base.core.ProfileData.Characters[value].Unlocked.ToString();
			dictionary[$"character-{value}-level"] = base.core.ProfileData.Characters[value].Level.ToString();
		}
		foreach (Achievement value2 in Enum.GetValues(typeof(Achievement)))
		{
			dictionary[$"achievment-{value2}"] = base.core.ProfileData.IsAchievementUnlocked(value2).ToString();
		}
		foreach (Stat value3 in Enum.GetValues(typeof(Stat)))
		{
			if (value3 != Stat.Sessions)
			{
				dictionary[$"stat-{value3}"] = _stat(value3).ToString();
			}
		}
		dictionary["remove-ads"] = base.core.ProfileData.AdsRemoved.ToString();
		dictionary["coin-doubler-enabled"] = base.core.ProfileData.CoinDoublerEnabled.ToString();
		return dictionary;
	}

	private void DownloadData()
	{
		downloading = true;
		Task<Dictionary<string, string>> task = Task.Run(() => PlatformDownload());
		task.GetAwaiter().OnCompleted(delegate
		{
			Dictionary<string, string> result = task.Result;
			ClearFields();
			if (result != null)
			{
				fields = result;
				if ((!base.core.ProfileData.InitiallyMerged && HasDataFromAnyDevice()) || HasDataFromOtherDevice())
				{
					MergeData();
				}
				UploadData();
			}
			downloading = false;
			downloads++;
			if (DidDownloaded != null)
			{
				DidDownloaded(this, EventArgs.Empty);
			}
		});
	}

	private void UploadData()
	{
		uploading = true;
		Dictionary<string, string> d = BuildDataForUploading();
		Task.Run(delegate
		{
			PlatformUpload(d);
		}).GetAwaiter().OnCompleted(delegate
		{
			uploading = false;
			if (!base.core.ProfileData.InitiallyMerged)
			{
				base.core.ProfileData.InitiallyMerged = true;
				base.core.ProfileData.SaveIntoStorage();
			}
			if (DidUploaded != null)
			{
				DidUploaded(this, EventArgs.Empty);
			}
			base.core.ProfileData.LastSyncTime = DateTimeHelper.SafeNow();
		});
	}

	public void Sync()
	{
		if (base.core.ProfileData.UseCloud && !downloading && !uploading && PlatformIsCloudAvailable())
		{
			DownloadData();
		}
	}

	public override void Update()
	{
		if (base.core.CurrentPlayState != null && downloads == 0)
		{
			Sync();
		}
		base.Update();
	}

	private bool GetBool(string key)
	{
		bool result = false;
		bool.TryParse(GetField(key), out result);
		return result;
	}

	private void SetBool(string key, bool value)
	{
		SetField(key, value.ToString());
	}

	private int GetInt(string key)
	{
		int result = 0;
		int.TryParse(GetField(key), out result);
		return result;
	}

	private void SetInt(string key, int value)
	{
		SetField(key, value.ToString());
	}

	private void SetField(string key, string value)
	{
		fields[key] = value;
	}

	private string GetField(string key)
	{
		if (!fields.ContainsKey(key))
		{
			return string.Empty;
		}
		return fields[key];
	}

	private void ClearFields()
	{
		fields.Clear();
	}
}
