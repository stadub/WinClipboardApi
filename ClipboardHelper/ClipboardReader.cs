using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.WinApi;
using Utils;

namespace ClipboardHelper
{
    public class ClipboardReader : IClipboardReader
    {
        private readonly Dictionary<string, uint> registeredFormats;
        private readonly Dictionary<string, Func<IClipbordFormatProvider>> formatProviders;

        private Clipboard clipboard;

        public ClipboardReader(Clipboard clipboard, Dictionary<string, uint> registeredFormats, Dictionary<string, Func<IClipbordFormatProvider>> formatProviders)
        {
            this.clipboard = clipboard;
            this.registeredFormats = registeredFormats;
            this.formatProviders = formatProviders;
        }

        public void GetData(IClipbordFormatProvider provider)
        {
            clipboard.GuardClipbordOpened();
            var formatId = clipboard.GetFormatId(provider.FormatId);
            if (!clipboard.IsDataAvailable(provider))
                throw new ClipboardDataException("There no data of selected format in the Clipboard", ExceptionHelpers.GetLastWin32Exception());

            IntPtr memHandle = ClipbordWinApi.GetClipboardData(formatId);
            if (memHandle == IntPtr.Zero)
                throw new ClipboardDataException("Can't receive data from clipbord", ExceptionHelpers.GetLastWin32Exception());

            try
            {
                using (var memory = new GlobalMemory(memHandle))
                {
                    int lenght = (int)memory.Size();
                    var memPtr = memory.Lock();
                    var buffer = new byte[lenght];
                    Marshal.Copy(memPtr, buffer, 0, lenght);

                    provider.Deserialize(buffer);
                }
            }
            catch (GlobalMemoryException exception)
            {
                throw new ClipboardDataException("Can't receive data from clipbord",exception);
            }
        }

        public IEnumerable<IClipbordFormatProvider> GetAvalibleFromats(bool includeUnknown=false)
        {
            clipboard.GuardClipbordOpened();
            List<uint> unknownformatsIds= new List<uint>();
            uint currentFormat = 0;
            var registeredIds= registeredFormats.ToDictionary(pair => pair.Value);

            var providers= new List<IClipbordFormatProvider>();
            while (true)
            {
                currentFormat = Clipboard.EnumClipboardFormats(currentFormat);
                if (currentFormat != 0)
                {
                    KeyValuePair<string, uint> registeredFormatId;
                    if (registeredIds.TryGetValue(currentFormat, out registeredFormatId))
                    {
                        if (formatProviders.ContainsKey(registeredFormatId.Key))
                        {
                            var formatProvider = formatProviders[registeredFormatId.Key];
                            providers.Add(formatProvider());
                        }
                        else
                            unknownformatsIds.Add(currentFormat);
                    }
                    else
                        unknownformatsIds.Add(currentFormat);
                }
                else
                {
                    var err = Marshal.GetLastWin32Error();
                    if (err == 0)
                    {
                        if (!includeUnknown)
                            return providers;

                        var unknownProviders=GetUnknownFormats(unknownformatsIds);
                        providers.AddRange(unknownProviders);
                        return providers;
                    }
                    throw ClipboardDataException.FromNative("Error in retreiving avalible clipbord formats");
                }
            }
        }

        private List<IClipbordFormatProvider> GetUnknownFormats(List<uint> unknownformatsIds)
        {
            var providers = new List<IClipbordFormatProvider>();
            var standartFormats = unknownformatsIds.Where(x => Enum.IsDefined(typeof (StandartClipboardFormats), x))
                .ToList();

            foreach (var standartFormat in standartFormats)
            {
                var notImplementedProvieder = new NotImplementedStandartFormat((StandartClipboardFormats) standartFormat);
                providers.Add(notImplementedProvieder);
            }
            foreach (var unknownformatId in unknownformatsIds.Except(standartFormats))
            {
                var formatNameBuilder = new StringBuilder(100);
                ClipbordWinApi.GetClipboardFormatName(unknownformatId, formatNameBuilder, 100);
                var unknownformatIdProvieder = new UnknownFormatProvider(unknownformatId, formatNameBuilder.ToString());
                providers.Add(unknownformatIdProvieder);
            }
            return providers;
        }

        public void Close()
        {
            clipboard.Close();
        }


        public void Dispose()
        {
            Close();
        }
    }
}