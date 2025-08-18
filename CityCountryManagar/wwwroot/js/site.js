// Show modal popup and load partial view from controller
function showInPopup(url, title) {
    $.get(url, function (data) {
        $("#form-modal .modal-body").html(data);
        $("#form-modal .modal-title").html(title);
        $("#form-modal").modal('show');
    });
}

// AJAX submit for form with success messages
function ajaxFormSubmit(form) {
    $.ajax({
        type: 'POST',
        url: $(form).attr('action'),
        data: $(form).serialize(),
        success: function (res) {
            if (res.isValid) {
                $("#view-all").html(res.html);
                $("#form-modal .modal-body").html('');
                $("#form-modal .modal-title").html('');
                $("#form-modal").modal('hide');
                
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
    if (confirm('Are you sure to delete this record?')) {
        console.log(`Attempting to delete ${controller} with ID: ${id}`);
        
        // Get the anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            // Try to get token from meta tag
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
                console.log('Delete response received:', res);
                
                if (res.success) {
                    // Always refresh the data after successful deletion
                    if (controller === "Employees") {
                        $.get(`/${controller}/GetEmployees`, function (data) {
                            $("#view-all").html(data);
                        });
                    } else if (controller === "Cities") {
                        $.get(`/${controller}/GetCities`, function (data) {
                            $("#view-all").html(data);
                        });
                    } else if (controller === "Countries") {
                        $.get(`/${controller}/GetCountries`, function (data) {
                            $("#view-all").html(data);
                        });
                    } else if (controller === "DailyRides") {
                        $.get(`/${controller}/GetDailyRides`, function (data) {
                            $("#view-all").html(data);
                        });
                    }
                    
                    if (res.message) {
                        showSuccessMessage(res.message);
                    } else {
                        showSuccessMessage("Record deleted successfully");
                    }
                } else {
                    // Show error message
                    showErrorMessage(res.message || "Failed to delete record");
                }
            },
            error: function(xhr, status, error) {
                console.error('Delete request failed:', {xhr, status, error});
                // Handle AJAX errors
                let errorMessage = "Error occurred while deleting record";
                
                // Try to parse error response for more details
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        errorMessage += ": " + error;
                    }
                } else {
                    errorMessage += ": " + error;
                }
                
                showErrorMessage(errorMessage);
            }
        });
    }
}

// Show success message
function showSuccessMessage(message) {
    // Create a temporary success alert
    var alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-check-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Add to body
    $('body').append(alertHtml);
    
    // Auto-remove after 3 seconds
    setTimeout(function() {
        $('.alert-success').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 3000);
}

// Show error message
function showErrorMessage(message) {
    // Create a temporary error alert
    var alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-exclamation-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Add to body
    $('body').append(alertHtml);
    
    // Auto-remove after 5 seconds
    setTimeout(function() {
        $('.alert-danger').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
}

// jQuery AJAX post with FormData (for file uploads)
function jQueryAjaxPost(form) {
    try {
        $.ajax({
            type: 'POST',
            url: $(form).attr('action'),
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-all').html(res.html);
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                    
                    // Show success message if available
                    if (res.message) {
                        showSuccessMessage(res.message);
                    }
                } else {
                    $('#form-modal .modal-body').html(res.html);
                }
            },
            error: function (err) {
                console.log(err);
                showErrorMessage("An error occurred while processing the request");
            }
        });
    } catch (ex) {
        console.log(ex);
        showErrorMessage("An error occurred while processing the request");
    }
    return false;
}

// jQuery AJAX delete
function jQueryAjaxDelete(url) {
    if (confirm('Are you sure to delete this record?')) {
        $.post(url, function (res) {
            if (res.success) {
                $('#view-all').html(res.html);
                showSuccessMessage(res.message || "Record deleted successfully");
            } else {
                showErrorMessage(res.message || "Failed to delete record");
            }
        }).fail(function(xhr, status, error) {
            console.error('Delete request failed:', {xhr, status, error});
            showErrorMessage("Error occurred while deleting record: " + error);
        });
    }
}
