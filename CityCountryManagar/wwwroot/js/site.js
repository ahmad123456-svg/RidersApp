// Show modal popup and load partial view from controller
function showInPopup(url, title) {
    $.get(url, function (data) {
        $("#form-modal .modal-body").html(data);
        $("#form-modal .modal-title").html(title);
        // Use Bootstrap 5 Modal API
        var modalElement = document.getElementById('form-modal');
        var modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
        modalInstance.show();
    });
}

// Try to reload any initialized DataTable on the page (server-side friendly)
function reloadActiveDataTable() {
    if (!$.fn || !$.fn.DataTable) return false;
    var reloaded = false;
    $('table.datatable').each(function() {
        if ($.fn.DataTable.isDataTable(this)) {
            var dt = $(this).DataTable();
            if (dt && dt.ajax && typeof dt.ajax.reload === 'function') {
                dt.ajax.reload(null, false);
                reloaded = true;
            }
        }
    });
    return reloaded;
}

// AJAX submit for form with success messages
function ajaxFormSubmit(form) {
    $.ajax({
        type: 'POST',
        url: $(form).attr('action'),
        data: $(form).serialize(),
        success: function (res) {
            if (res.isValid) {
                // Prefer reloading existing server-side DataTables
                var didReload = reloadActiveDataTable();
                if (!didReload && res.html) {
                    // Fallback for legacy client-side partial updates
                    $("#view-all").html(res.html);
                }

                $("#form-modal .modal-body").html('');
                $("#form-modal .modal-title").html('');
                // Hide using Bootstrap 5 API
                var modalElement = document.getElementById('form-modal');
                var modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
                modalInstance.hide();
                
                // Show success message if available
                if (res.message) {
                    showSuccessMessage(res.message);
                }
            } else {
                $("#form-modal .modal-body").html(res.html);
            }
        }
    });
    return false;
}

// Delete record using controller name and record ID
function deleteRecord(controller, id) {
    if (confirm('Are you sure you want to delete this record?')) {
        // Get the anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            token = $('meta[name="__RequestVerificationToken"]').attr('content');
        }
        
        $.ajax({
            url: `/${controller}/Delete/${id}`,
            type: 'POST',
            data: { __RequestVerificationToken: token },
            headers: {
                'RequestVerificationToken': token
            },
            success: function(res) {
                if (res.success) {
                    // Prefer reloading existing server-side DataTables
                    var didReload = reloadActiveDataTable();
                    if (!didReload && res.html) {
                        // Fallback: replace content
                        $("#view-all").html(res.html);
                    } else if (!didReload && !res.html) {
                        // Last resort
                        location.reload();
                    }
                    
                    if (res.message) {
                        showSuccessMessage(res.message);
                    } else {
                        showSuccessMessage("Record deleted successfully");
                    }
                } else {
                    showErrorMessage(res.message || "Failed to delete record");
                }
            },
            error: function(xhr, status, error) {
                console.error('Delete request failed:', {xhr, status, error});
                showErrorMessage("Error occurred while deleting record: " + error);
            }
        });
    }
}

// Show success message
function showSuccessMessage(message) {
    var alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-check-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    setTimeout(function() {
        $('.alert-success').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 3000);
}

// Show error message
function showErrorMessage(message) {
    var alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-exclamation-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    setTimeout(function() {
        $('.alert-danger').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
}

$(document).ready(function () {
    // Cascading dropdown for Employee Add/Edit
    $(document).on('change', '#CountryId', function () {
        var countryId = $(this).val();
        $('#CityId').empty().append('<option value="">-- Select City --</option>');
        if (countryId) {
            $.getJSON('/Cities/GetCitiesByCountry', { countryId: countryId }, function (data) {
                $.each(data, function (i, city) {
                    $('#CityId').append($('<option>', {
                        value: city.cityId,
                        text: city.cityName
                    }));
                });
            });
        }
    });
});
