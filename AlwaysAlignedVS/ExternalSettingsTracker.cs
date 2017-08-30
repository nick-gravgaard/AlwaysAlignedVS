using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Linq;

namespace AlwaysAligned
{
	internal class ExternalSettingsTracker
	{
		public class TextManagerEvents : IVsTextManagerEvents
		{
			public void OnRegisterMarkerType(int iMarkerType)
			{
			}

			public void OnRegisterView(IVsTextView pView)
			{
			}

			public void OnUnregisterView(IVsTextView pView)
			{
			}

			private FONTCOLORPREFERENCES[] _pColorPrefs;
			public void OnUserPreferencesChanged(VIEWPREFERENCES[] pViewPrefs, FRAMEPREFERENCES[] pFramePrefs, LANGPREFERENCES[] pLangPrefs, FONTCOLORPREFERENCES[] pColorPrefs)
			{
				if (pColorPrefs == null || !pColorPrefs.Any())
				{
					_pColorPrefs = null;
					return;
				}
				if (_pColorPrefs != null
					&& _pColorPrefs.Count() == pColorPrefs.Count()
					&& _pColorPrefs.Zip(pColorPrefs, (fr1, fr2) => new Tuple<FONTCOLORPREFERENCES, FONTCOLORPREFERENCES>(fr1, fr2)).All(
						t => CheckFontColorReferencesAreEqual(t.Item1, t.Item2)))
				{
					return;
				}

				_pColorPrefs = pColorPrefs;

				AlwaysAlignedConfigurationService.Instance.OnExternalConfigurationChanged();
			}

			private bool CheckFontColorReferencesAreEqual(FONTCOLORPREFERENCES fr1, FONTCOLORPREFERENCES fr2)
			{
				return fr1.hBoldViewFont == fr2.hBoldViewFont
					   && fr1.hRegularViewFont == fr2.hRegularViewFont
					   && fr1.pguidColorCategory == fr2.pguidColorCategory
					   && fr1.pguidFontCategory == fr2.pguidFontCategory;
			}
		}

		public static void Start()
		{
			var textManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;

			var container = (IConnectionPointContainer)textManager;
			IConnectionPoint textManagerEventsConnection = null;
			var eventGuid = typeof(IVsTextManagerEvents).GUID;
			if (container != null)
			{
				container.FindConnectionPoint(ref eventGuid, out textManagerEventsConnection);
			}
			var textManagerEvents = new TextManagerEvents();
			uint textManagerCookie;
			if (textManagerEventsConnection != null)
			{
				textManagerEventsConnection.Advise(textManagerEvents, out textManagerCookie);
			}
		}
	}
}
