// Show modal popup and load partial view from controller
function showInPopup(url, title) {
    console.log('showInPopup called with URL:', url);
    console.log('showInPopup called with title:', title);
    
    $.get(url)
        .done(function (data) {
            console.log('Successfully loaded popup content');
            $("#form-modal .modal-body").html(data);
            $("#form-modal .modal-title").html(title);
            // Use Bootstrap 5 Modal API
            var modalElement = document.getElementById('form-modal');
            var modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
            modalInstance.show();
        })
        .fail(function (xhr, status, error) {
            console.error('Failed to load popup:', {xhr, status, error});
            console.error('Response text:', xhr.responseText);
            console.error('Status code:', xhr.status);
            
            // Show error message to user
            var errorMessage = 'Failed to load form';
            if (xhr.status === 403) {
                errorMessage = 'Access denied. You do not have permission to access this feature.';
            } else if (xhr.status === 404) {
                errorMessage = 'The requested page was not found.';
            } else if (xhr.status === 500) {
                errorMessage = 'A server error occurred. Please try again later.';
            }
            
            showErrorMessage(errorMessage);
        });
}

// Try to reload any initialized DataTable on the page (server-side friendly)
function reloadActiveDataTable() {
    if (!$.fn || !$.fn.DataTable) return false;
    var reloaded = false;
    
    // Check for specific DataTable instances stored in window object
    if (window.usersDataTable && typeof window.usersDataTable.ajax.reload === 'function') {
        window.usersDataTable.ajax.reload(null, false);
        reloaded = true;
    }
    
    // Check for other DataTables with server-side processing
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

// AJAX submit for form with enhanced success message handling
function ajaxFormSubmit(form) {
    console.log('ajaxFormSubmit called');
    console.log('Form action:', $(form).attr('action'));
    
    // Prevent double submission
    var submitButton = $(form).find('button[type="submit"]');
    var originalText = submitButton.html();
    submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    
    $.ajax({
        type: 'POST',
        url: $(form).attr('action'),
        data: $(form).serialize(),
        success: function (res) {
            console.log('AJAX Success Response:', res);
            
            if (res.isValid) {
                console.log('Form submission successful');
                
                // Close modal immediately
                var modalElement = document.getElementById('form-modal');
                if (modalElement) {
                    var modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
                    modalInstance.hide();
                    
                    // Clear modal content
                    $("#form-modal .modal-body").html('');
                    $("#form-modal .modal-title").html('');
                }
                
                // Show success message after a short delay
                setTimeout(function() {
                    if (res.message) {
                        showSuccessMessage(res.message);
                    }
                }, 300);
                
                // Reload DataTable
                setTimeout(function() {
                    reloadActiveDataTable();
                }, 500);
                
            } else {
                console.log('Form validation failed, updating modal content');
                $("#form-modal .modal-body").html(res.html);
                
                // Re-enable submit button
                submitButton.prop('disabled', false).html(originalText);
            }
        },
        error: function(xhr, status, error) {
            console.error('AJAX Error:', {xhr, status, error});
            showErrorMessage('An error occurred while saving. Please try again.');
            
            // Re-enable submit button
            submitButton.prop('disabled', false).html(originalText);
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
                    // Always try to reload DataTable first
                    var didReload = reloadActiveDataTable();
                    
                    // If no DataTable was reloaded and we have HTML content, use fallback
                    if (!didReload && res.html) {
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
    // Remove any existing success messages first
    $('.alert-success').remove();
    
    var alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert" style="position: fixed; top: 80px; right: 20px; z-index: 10000; min-width: 350px; max-width: 500px;">
            <i class="fas fa-check-circle me-2"></i>
            <strong>Success!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    // Auto-remove after 4 seconds
    setTimeout(function() {
        $('.alert-success').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 4000);
}

// Show error message
function showErrorMessage(message) {
    // Remove any existing error messages first
    $('.alert-danger').remove();
    
    var alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert" style="position: fixed; top: 80px; right: 20px; z-index: 10000; min-width: 350px; max-width: 500px;">
            <i class="fas fa-exclamation-circle me-2"></i>
            <strong>Error!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    $('body').append(alertHtml);
    
    // Auto-remove after 6 seconds
    setTimeout(function() {
        $('.alert-danger').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 6000);
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
    
    // Modal cleanup when hidden
    $('#form-modal').on('hidden.bs.modal', function () {
        $(this).find('.modal-body').html('');
        $(this).find('.modal-title').html('');
        console.log('Modal hidden and cleaned up');
    });
});
