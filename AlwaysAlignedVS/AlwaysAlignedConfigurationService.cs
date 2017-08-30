using AlwaysAligned.Properties;
using System;

namespace AlwaysAligned
{
	public class AlwaysAlignedConfiguration : ICloneable
	{
		public int MinimumCellWidth { get; set; }
		public int CellPadding { get; set; }
		public bool Enabled { get; set; }
		public bool ConvertOnLoadSave { get; set; }

		public object Clone()
		{
			return new AlwaysAlignedConfiguration
			{
				ConvertOnLoadSave = ConvertOnLoadSave,
				Enabled = Enabled,
				MinimumCellWidth = MinimumCellWidth,
				CellPadding = CellPadding
			};
		}
	}

	public class ConfigurationSavedEventArgs : EventArgs
	{
		public bool HasChanges { get; internal set; }
	}

	public class AlwaysAlignedConfigurationService
	{
		static readonly Lazy<AlwaysAlignedConfigurationService> LazyInstance = new Lazy<AlwaysAlignedConfigurationService>(() => new AlwaysAlignedConfigurationService());

		private readonly WeakEvent<EventHandler<ConfigurationSavedEventArgs>> _configurationSavedWeakEvent = new WeakEvent<EventHandler<ConfigurationSavedEventArgs>>();

		internal event EventHandler<ConfigurationSavedEventArgs> ConfigurationChanged
		{
			add
			{
				_configurationSavedWeakEvent.AddHandler(value);
			}
			remove
			{
				_configurationSavedWeakEvent.RemoveHandler(value);
			}
		}

		private AlwaysAlignedConfigurationService()
		{

		}

		public static AlwaysAlignedConfigurationService Instance
		{
			get
			{
				return LazyInstance.Value;
			}
		}

		internal AlwaysAlignedConfiguration Config;

		public AlwaysAlignedConfiguration GetConfiguration()
		{
			if (Config == null)
			{
				Config = new AlwaysAlignedConfiguration
				{
					Enabled = Settings.Default.Enabled,
					ConvertOnLoadSave = Settings.Default.ConvertOnLoadSave,
					MinimumCellWidth = Settings.Default.MinimumCellWidth,
					CellPadding = Settings.Default.CellPadding
				};
			}
			return Config;
		}

		public void Save()
		{
			Save(Config);
		}

		public void Save(AlwaysAlignedConfiguration config)
		{
			bool hasChanges = Settings.Default.Enabled != config.Enabled;
			Settings.Default.Enabled = config.Enabled;

			hasChanges = hasChanges || (Settings.Default.ConvertOnLoadSave != config.ConvertOnLoadSave);
			Settings.Default.ConvertOnLoadSave = config.ConvertOnLoadSave;

			hasChanges = hasChanges || (Settings.Default.MinimumCellWidth != config.MinimumCellWidth);
			Settings.Default.MinimumCellWidth = (int)config.MinimumCellWidth;

			hasChanges = hasChanges || (Settings.Default.CellPadding != config.CellPadding);
			Settings.Default.CellPadding = (int)config.CellPadding;

			Settings.Default.Save();

			//Refresh config
			Config = null;
			OnConfigurationSaved(new ConfigurationSavedEventArgs { HasChanges = hasChanges });
		}

		public string GetSettingsAsString()
		{
			var templ = "Enabled: {0}, ConvertOnLoadSave: {1}, MinimumCellWidth: {2}, CellPadding: {3}";
			if (Config == null)
			{
				return string.Format(templ,
					Settings.Default.Enabled,
					Settings.Default.ConvertOnLoadSave,
					Settings.Default.MinimumCellWidth,
					Settings.Default.CellPadding
				);
			}
			else
			{
				return string.Format(templ,
					Config.Enabled,
					Config.ConvertOnLoadSave,
					Config.MinimumCellWidth,
					Config.CellPadding
				);
			}
		}

		public void OnExternalConfigurationChanged()
		{
			OnConfigurationSaved(new ConfigurationSavedEventArgs { HasChanges = true });
		}

		private void OnConfigurationSaved(ConfigurationSavedEventArgs eventArgs)
		{
			_configurationSavedWeakEvent.Raise(this, eventArgs);
		}
	}
}
