// Global modal instance
let formModal;

// Universal function to close modal
function closeModal() {
    console.log('closeModal function called');
    
    // Try different methods to close the modal
    var modalElement = document.getElementById('form-modal');
    if (!modalElement) {
        console.error('Modal element #form-modal not found');
        return false;
    }
    
    console.log('Modal element found:', modalElement);
    console.log('Modal classes:', modalElement.className);
    
    var modalClosed = false;
    
    try {
        // Method 1: Use global formModal instance if available
        if (window.formModal) {
            console.log('Using global formModal instance');
            window.formModal.hide();
            modalClosed = true;
        }
    } catch (e) {
        console.error('Global formModal hide failed:', e);
    }
    
    try {
        // Method 2: Bootstrap 5 getInstance
        if (!modalClosed) {
            var modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                console.log('Closing modal using existing Bootstrap instance');
                modal.hide();
                modalClosed = true;
            }
        }
    } catch (e) {
        console.error('Bootstrap modal hide failed:', e);
    }
    
    try {
        // Method 3: Create new Bootstrap instance
        if (!modalClosed) {
            console.log('Creating new Bootstrap modal instance');
            var newModal = new bootstrap.Modal(modalElement);
            newModal.hide();
            modalClosed = true;
        }
    } catch (e) {
        console.error('Bootstrap new modal hide failed:', e);
    }
    
    try {
        // Method 4: jQuery method
        if (!modalClosed) {
            console.log('Trying jQuery modal hide');
            $('#form-modal').modal('hide');
            modalClosed = true;
        }
    } catch (e) {
        console.error('jQuery modal hide failed:', e);
    }
    
    // Method 5: Force hide using CSS and DOM manipulation (always as backup)
    setTimeout(function() {
        console.log('Force hiding modal as backup');
        var modal = document.getElementById('form-modal');
        if (modal) {
            modal.style.display = 'none';
            modal.classList.remove('show');
            modal.setAttribute('aria-hidden', 'true');
            modal.removeAttribute('aria-modal');
            modal.removeAttribute('role');
        }
        
        // Remove backdrop
        var backdrops = document.querySelectorAll('.modal-backdrop');
        console.log('Found backdrops:', backdrops.length);
        backdrops.forEach(function(backdrop) {
            backdrop.remove();
        });
        
        // Remove modal-open class from body
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
        
        console.log('Modal force closed');
    }, modalClosed ? 100 : 0); // Immediate if no other method worked
    
    return true;
}

// Test function to verify closeModal is accessible
window.testCloseModal = function() {
    console.log('Testing closeModal function...');
    closeModal();
};

// Show modal popup and load partial view from controller
function showInPopup(url, title) {
    console.log('showInPopup called with URL:', url);
    console.log('showInPopup called with title:', title);
    
    // Get modal element
    const modalElement = document.getElementById('form-modal');
    
    // Create or get modal instance and make it globally accessible
    if (!window.formModal) {
        window.formModal = new bootstrap.Modal(modalElement, {
            backdrop: 'static',
            keyboard: false
        });
    }
    
    // Show loading state
    $("#form-modal .modal-body").html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
    $("#form-modal .modal-title").html(title);
    window.formModal.show();
    
    // Fetch content
    $.get(url)
        .done(function (data) {
            console.log('Successfully loaded popup content');
            $("#form-modal .modal-body").html(data);
            
            // Initialize any form features
            if (typeof initializeFineOrExpenseForm === 'function') {
                initializeFineOrExpenseForm();
            }
            
            // Focus first input
            setTimeout(function() {
                $("#form-modal input:visible, #form-modal select:visible").first().focus();
            }, 100);
        })
        .fail(function (xhr, status, error) {
            console.error('Failed to load popup:', {xhr, status, error});
            
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
    console.log('Form element:', form);
    console.log('Form action:', $(form).attr('action'));
    console.log('Form method:', $(form).attr('method'));
    console.log('Form data:', $(form).serialize());
    
    // Get form data as object for debugging
    var formData = {};
    $(form).serializeArray().forEach(function(item) {
        formData[item.name] = item.value;
    });
    console.log('Form data object:', formData);
    
    // Prevent form from submitting normally
    if (event) {
        event.preventDefault();
    }
    
    // Check if form is already being submitted
    if ($(form).data('submitting')) {
        console.log('Form already being submitted, ignoring');
        return false;
    }
    
    // Mark form as being submitted
    $(form).data('submitting', true);
    
    // Prevent double submission
    var submitButton = $(form).find('button[type="submit"]');
    var originalText = submitButton.html();
    submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    
    // Check if form has file uploads
    var hasFileUpload = $(form).find('input[type="file"]').length > 0;
    var requestData;
    var contentType;
    var processData;
    
    if (hasFileUpload) {
        // Use FormData for file uploads
        requestData = new FormData(form);
        contentType = false;
        processData = false;
        console.log('Using FormData for file upload');
    } else {
        // Use serialized data for regular forms
        requestData = $(form).serialize();
        contentType = 'application/x-www-form-urlencoded; charset=UTF-8';
        processData = true;
        console.log('Using serialized data for regular form');
    }
    
    $.ajax({
        type: 'POST',
        url: $(form).attr('action'),
        data: requestData,
        contentType: contentType,
        processData: processData,
        success: function (res) {
            console.log('AJAX Success Response:', res);
            console.log('Response isValid:', res.isValid);
            console.log('Response message:', res.message);
            
            // Remove submitting flag
            $(form).data('submitting', false);
            
            if (res.isValid) {
                console.log('Form submission successful');
                
                // Re-enable submit button first
                submitButton.prop('disabled', false).html(originalText);
                
                // Show success message once
                if (res.message) {
                    console.log('Showing success message:', res.message);
                    showSuccessMessage(res.message);
                }
                
                // Close modal using universal closeModal function
                console.log('Calling closeModal from ajaxFormSubmit');
                console.log('Checking if closeModal function exists:', typeof closeModal);
                
                // Clear file inputs after successful submission
                $(form).find('input[type="file"]').val('');
                console.log('Cleared file inputs');
                
                if (typeof closeModal === 'function') {
                    closeModal();
                } else {
                    console.error('closeModal function not found! Using fallback...');
                    // Fallback method
                    if (window.formModal) {
                        window.formModal.hide();
                    } else {
                        $('#form-modal').modal('hide');
                    }
                }
                
                // Clear modal content after modal closes
                setTimeout(function() {
                    console.log('Clearing modal content');
                    $("#form-modal .modal-body").empty();
                    $("#form-modal .modal-title").empty();
                }, 500);
                
                // Reload DataTable
                setTimeout(function() {
                    reloadActiveDataTable();
                }, 500);
            } else {
                console.log('Form validation or submission failed');
                
                // Remove submitting flag
                $(form).data('submitting', false);
                
                // Re-enable submit button
                submitButton.prop('disabled', false).html(originalText);
                
                // Update form with validation errors if any
                if (res.html) {
                    $("#form-modal .modal-body").html(res.html);
                    
                    // Re-initialize form validation
                    if (typeof initializeFineOrExpenseForm === 'function') {
                        initializeFineOrExpenseForm();
                    }
                    
                    // Re-attach form handlers after updating content
                    attachFormHandlers();
                    
                    // Focus on first invalid field
                    setTimeout(function() {
                        $('.is-invalid').first().focus();
                    }, 100);
                }
                
                // Show error message once only
                if (res.message) {
                    showErrorMessage(res.message);
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('AJAX Error:', {xhr, status, error});
            
            // Remove submitting flag
            $(form).data('submitting', false);
            
            // Re-enable submit button
            submitButton.prop('disabled', false).html(originalText);
            
            var errorMessage = 'An error occurred while saving. Please try again.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            
            showErrorMessage(errorMessage);
        }
    });
    return false;
}

// Delete record using controller name and record ID
function deleteRecord(controller, id) {
    // Prevent multiple confirmations by checking if already processing
    if (window.isDeleting) {
        console.log('Delete already in progress, ignoring');
        return;
    }
    
    if (confirm('Are you sure you want to delete this record?')) {
        console.log(`Attempting to delete ${controller} record with ID: ${id}`);
        
        // Set flag to prevent multiple deletes
        window.isDeleting = true;
        
        // Get the anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            token = $('meta[name="__RequestVerificationToken"]').attr('content');
        }
        
        console.log('Anti-forgery token:', token);
        
        // Disable all action buttons for this record temporarily
        var actionButtons = $(`[data-id="${id}"]`);
        actionButtons.prop('disabled', true);
        
        $.ajax({
            url: `/${controller}/Delete`,
            type: 'POST',
            data: { 
                id: id,
                __RequestVerificationToken: token
            },
            headers: {
                'RequestVerificationToken': token
            },
            success: function(res) {
                console.log('Delete response:', res);
                
                // Reset delete flag
                window.isDeleting = false;
                
                if (res.success) {
                    // Show success message
                    showSuccessMessage(res.message || "Record deleted successfully");
                    
                    // Reload the DataTable
                    setTimeout(function() {
                        reloadActiveDataTable();
                    }, 500);
                } else {
                    showErrorMessage(res.message || "Failed to delete record");
                    // Re-enable buttons on failure
                    actionButtons.prop('disabled', false);
                }
            },
            error: function(xhr, status, error) {
                console.error('Delete Error:', {xhr, status, error});
                console.error('Response Text:', xhr.responseText);
                console.error('Status Code:', xhr.status);
                
                // Reset delete flag
                window.isDeleting = false;
                
                var errorMessage = 'An error occurred while deleting the record';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    errorMessage = `Server error: ${xhr.responseText}`;
                }
                
                showErrorMessage(errorMessage);
                // Re-enable buttons on error
                actionButtons.prop('disabled', false);
            }
        });
    } else {
        // User cancelled, reset any flags
        window.isDeleting = false;
    }
}

// Show success message
function showSuccessMessage(message) {
    console.log('Showing success message:', message);
    
    // Remove any existing success messages first
    $('.alert-success').remove();
    
    var alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert" style="position: fixed; top: 80px; right: 20px; z-index: 10000; min-width: 350px; max-width: 500px;">
            <i class="fas fa-check-circle me-2"></i>
            <strong>Success!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Add the alert to the body
    var alertElement = $(alertHtml).appendTo('body');
    
    // Auto-remove after 5 seconds
    setTimeout(function() {
        alertElement.fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
}

// Show error message
function showErrorMessage(message) {
    console.log('Showing error message:', message);
    
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

// Show toast notification
function showToast(title, message, type = 'success') {
    // Remove any existing toasts
    $('.toast').remove();

    // Create toast HTML
    const toastHtml = `
        <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header bg-${type === 'success' ? 'success' : 'danger'} text-white">
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    // Add toast to container
    $('.toast-container').append(toastHtml);

    // Initialize and show toast
    const toastElement = document.querySelector('.toast:last-child');
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 5000
    });
    toast.show();
}

$(document).ready(function () {
    // Initialize modal instance globally
    const modalElement = document.getElementById('form-modal');
    if (modalElement) {
        window.formModal = new bootstrap.Modal(modalElement, {
            backdrop: 'static',
            keyboard: false
        });
    }

    // Handle modal events
    if (modalElement) {
        modalElement.addEventListener('hidden.bs.modal', function () {
            // Clear modal content completely
            $("#form-modal .modal-body").empty();
            $("#form-modal .modal-title").empty();
            
            // Remove any validation messages
            $('.validation-summary-errors').remove();
            $('.input-validation-error').removeClass('input-validation-error');
            $('.is-invalid').removeClass('is-invalid');
            
            console.log('Modal hidden and cleaned up');
        });
    }

    // Handle close button clicks
    $(document).on('click', '[data-bs-dismiss="modal"]', function() {
        console.log('Close button clicked');
        if (window.formModal) {
            window.formModal.hide();
        } else {
            $('#form-modal').modal('hide');
        }
    });

    // Handle cancel button clicks specifically
    $(document).on('click', '.btn-secondary[data-bs-dismiss="modal"]', function() {
        console.log('Cancel button clicked');
        if (window.formModal) {
            window.formModal.hide();
        } else {
            $('#form-modal').modal('hide');
        }
    });

    // Handle modal trigger buttons
    $(document).on('click', '.show-form-modal, .edit-btn', function(e) {
        e.preventDefault();
        const url = $(this).data('url');
        const title = $(this).data('title');
        if (url) {
            showInPopup(url, title);
        }
    });

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
