(function ($) {
    var defaultMasks = {
        pint: /[\d]/,
        'int': /[\d\-]/,
        pnum: /[\d\.]/,
        money: /[\d\.\s,]/,
        num: /[\d\-\.]/,
        hex: /[0-9a-f]/i,
        email: /[a-z0-9_\.\-@]/i,
        alpha: /[a-z_]/i,
        alphanum: /[a-z0-9_]/i
    };

    var NAV_KEYS = new Set([
        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
        'Home', 'End', 'Tab', 'Escape', 'Enter'
    ]);

    function isPrintableKey(e) {
        // tasti “carattere”: una sola codepoint, nessun modificatore
        return !!e.key && e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey;
    }

    function testChar(reOrFn, ch, el) {
        if (!ch) return true;
        return (typeof reOrFn === 'function') ? !!reOrFn.call(el, ch) : reOrFn.test(ch);
    }

    $.fn.keyfilter = function (reOrFn) {
        return this.each(function () {
            var el = this;
            var $el = $(el);

            // KEYDOWN: blocco per carattere digitato
            $el.on('keydown', function (e) {
                if (NAV_KEYS.has(e.key) || e.key === 'Process') return; // 'Process' = IME
                if (!isPrintableKey(e)) return;

                if (!testChar(reOrFn, e.key, el)) {
                    e.preventDefault();
                }
            });

            // BEFOREINPUT: copre IME/mobile e paste “tardivi”
            $el.on('beforeinput', function (e) {
                var ev = e.originalEvent || e;
                var type = ev.inputType;
                if (type !== 'insertText' && type !== 'insertFromPaste') return;

                var data = ev.data;
                if (!data) return;

                for (var i = 0; i < data.length; i++) {
                    if (!testChar(reOrFn, data[i], el)) { e.preventDefault(); return; }
                }
            });

            // PASTE fallback (vecchi browser / casi particolari)
            $el.on('paste', function (e) {
                var txt = (e.originalEvent && e.originalEvent.clipboardData
                    ? e.originalEvent.clipboardData
                    : window.clipboardData).getData('text');
                if (!txt) return;

                for (var i = 0; i < txt.length; i++) {
                    if (!testChar(reOrFn, txt[i], el)) { e.preventDefault(); return; }
                }
            });
        });
    };

    $.extend($.fn.keyfilter, {
        defaults: { masks: defaultMasks },
        version: '2.0.0'
    });

    // Inizializzazione come nel tuo codice: attacca per classi .mask-*
    $(function () {
        var tags = $('input[class*=mask],textarea[class*=mask]');
        var masks = $.fn.keyfilter.defaults.masks;
        for (var key in masks) {
            if (Object.prototype.hasOwnProperty.call(masks, key)) {
                tags.filter('.mask-' + key).keyfilter(masks[key]);
            }
        }
    });

})(jQuery);