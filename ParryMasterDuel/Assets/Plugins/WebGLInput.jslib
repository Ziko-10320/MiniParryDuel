mergeInto(LibraryManager.library, {
  RequestKeyboardCapture: function() {
    window.addEventListener("keydown", function(e) {
      if(["M","ArrowUp","ArrowDown","ArrowLeft","ArrowRight"].indexOf(e.code) > -1) {
        e.preventDefault();
      }
    }, false);
  }
});
