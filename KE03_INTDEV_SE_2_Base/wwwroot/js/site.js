// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    var searchTimeout;
    var $searchInput = $('#searchInput');
    var $searchSuggestions = $('#searchSuggestions');

    $searchInput.on('input', function () {
        clearTimeout(searchTimeout);
        var searchTerm = $(this).val();

        if (searchTerm.length < 2) {
            $searchSuggestions.addClass('d-none').empty();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.get('/Home/SearchSuggestions', { term: searchTerm }, function (data) {
                $searchSuggestions.empty();

                if (data.length > 0) {
                    data.forEach(function (item) {
                        var itemClass = item.type === 'product' ? 'text-primary' : 'text-success';
                        var icon = item.type === 'product' ? 'box' : 'gear';
                        var $suggestion = $('<div>')
                            .addClass('p-2 suggestion-item cursor-pointer')
                            .html(`<i class="fa-solid fa-${icon} me-2 ${itemClass}"></i>${item.label}`)
                            .data('item', item);

                        $suggestion.on('click', function () {
                            var clickedItem = $(this).data('item');
                            if (clickedItem.type === 'product') {
                                window.location.href = '/Product/Details/' + clickedItem.value;
                            } else {
                                window.location.href = '/Part/Details/' + clickedItem.value;
                            }
                        });

                        $searchSuggestions.append($suggestion);
                    });

                    $searchSuggestions.removeClass('d-none');
                } else {
                    $searchSuggestions.addClass('d-none');
                }
            });
        }, 300);
    });

    // Close suggestions when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.position-relative').length) {
            $searchSuggestions.addClass('d-none');
        }
    });

    // Add hover effect styles
    var style = `
        .suggestion-item:hover {
            background-color: #f8f9fa;
            cursor: pointer;
        }
    `;

    $('<style>').text(style).appendTo('head');
});
