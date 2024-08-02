

mergeInto(LibraryManager.library, {
  CopyToClipboardJS:function(text) {
    if (navigator.clipboard) {
        var toCopyStr = UTF8ToString(text);
      navigator.clipboard.writeText(toCopyStr).then(function () {
        console.log('Text copied to clipboard');
        console.log(toCopyStr);
      }).catch(function (err) {
        console.error('Could not copy text: ', err);
      });
    } else {
      console.warn('Clipboard API not supported');
    }
  },

   OpenTelegramShareURL : function()
    {
          var url =  "https://t.me/wifwhack_bot?start=myReferId";
        var text = "Play to Wif Whack!";
        var shareUrl = "https://t.me/share/url?url=" + url + "&text="+text;
        window.open(encodeURI(shareUrl));
    },

});