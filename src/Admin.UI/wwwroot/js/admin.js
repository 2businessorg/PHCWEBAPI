// Admin Panel JavaScript - SPA-like functionality

// Toast Notifications
class ToastManager {
    static show(message, type = 'info') {
        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            warning: 'fa-exclamation-triangle',
            info: 'fa-info-circle'
        };

        const colors = {
            success: 'bg-green-500',
            error: 'bg-red-500',
            warning: 'bg-yellow-500',
            info: 'bg-blue-500'
        };

        const toast = document.createElement('div');
        toast.className = `${colors[type]} text-white px-6 py-4 rounded-lg shadow-lg flex items-center space-x-3 transform transition-all duration-300 translate-x-full opacity-0`;
        toast.innerHTML = `
            <i class="fas ${icons[type]} text-xl"></i>
            <span class="font-medium">${message}</span>
            <button class="ml-4 text-white hover:text-gray-200" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        `;

        const container = document.getElementById('toast-container');
        container.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.classList.remove('translate-x-full', 'opacity-0');
        }, 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.classList.add('translate-x-full', 'opacity-0');
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    }
}

// API Client
class APIClient {
    constructor(baseURL = '/api/authenticate') {
        this.baseURL = baseURL;
        this.token = localStorage.getItem('admin_token');
    }

    async request(endpoint, options = {}) {
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(`${this.baseURL}${endpoint}`, {
                ...options,
                headers
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'Request failed');
            }

            return data;
        } catch (error) {
            ToastManager.show(error.message, 'error');
            throw error;
        }
    }

    // User Management
    async getUsers() {
        return this.request('/users');
    }

    async createUser(userData) {
        return this.request('/register', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    }

    async deleteUser(userId) {
        return this.request(`/users/${userId}`, {
            method: 'DELETE'
        });
    }

    // Role Management
    async getRoles() {
        return this.request('/roles');
    }

    async createRole(roleName) {
        return this.request('/roles', {
            method: 'POST',
            body: JSON.stringify({ roleName })
        });
    }

    async getUserRoles(username) {
        return this.request(`/users/${username}/roles`);
    }

    async addRoleToUser(username, role) {
        return this.request('/users/add-role', {
            method: 'POST',
            body: JSON.stringify({ username, role })
        });
    }

    async removeRoleFromUser(username, role) {
        return this.request('/users/remove-role', {
            method: 'POST',
            body: JSON.stringify({ username, role })
        });
    }
}

// Form Validation
class FormValidator {
    static validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    static validatePassword(password) {
        // At least 6 characters, 1 uppercase, 1 lowercase, 1 digit
        return password.length >= 6 && 
               /[A-Z]/.test(password) && 
               /[a-z]/.test(password) && 
               /[0-9]/.test(password);
    }

    static validateUsername(username) {
        // Alphanumeric, 3-20 characters
        return /^[a-zA-Z0-9_]{3,20}$/.test(username);
    }

    static showError(inputElement, message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'text-red-500 text-sm mt-1';
        errorDiv.textContent = message;
        
        const existingError = inputElement.parentElement.querySelector('.text-red-500');
        if (existingError) existingError.remove();
        
        inputElement.classList.add('border-red-500');
        inputElement.parentElement.appendChild(errorDiv);
    }

    static clearError(inputElement) {
        const errorDiv = inputElement.parentElement.querySelector('.text-red-500');
        if (errorDiv) errorDiv.remove();
        inputElement.classList.remove('border-red-500');
    }
}

// Modal Manager
class ModalManager {
    static show(title, content, actions = []) {
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 z-50 overflow-y-auto';
        modal.innerHTML = `
            <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
                <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75" onclick="this.parentElement.parentElement.remove()"></div>
                
                <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full animate-slide-in">
                    <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                        <div class="sm:flex sm:items-start">
                            <div class="mt-3 text-center sm:mt-0 sm:text-left w-full">
                                <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                                    ${title}
                                </h3>
                                <div class="mt-2">
                                    ${content}
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse space-x-2">
                        ${actions.map(action => `
                            <button type="button" class="${action.class}" onclick="${action.onclick}">
                                ${action.label}
                            </button>
                        `).join('')}
                        <button type="button" class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none sm:mt-0 sm:w-auto sm:text-sm" onclick="this.closest('.fixed').remove()">
                            Cancelar
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        return modal;
    }

    static confirm(title, message, onConfirm) {
        const actions = [{
            label: 'Confirmar',
            class: 'w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 btn-primary text-white text-base font-medium hover:shadow-lg focus:outline-none sm:ml-3 sm:w-auto sm:text-sm',
            onclick: `(async () => { 
                await (${onConfirm.toString()})(); 
                document.querySelector('.fixed.inset-0.z-50').remove(); 
            })()`
        }];

        return this.show(title, `<p class="text-sm text-gray-500">${message}</p>`, actions);
    }
}

// Loading Spinner
class LoadingManager {
    static show(element) {
        const spinner = document.createElement('div');
        spinner.className = 'inline-block animate-spin rounded-full h-5 w-5 border-b-2 border-white';
        spinner.id = 'loading-spinner';
        
        const originalContent = element.innerHTML;
        element.setAttribute('data-original-content', originalContent);
        element.innerHTML = '';
        element.appendChild(spinner);
        element.disabled = true;
    }

    static hide(element) {
        const originalContent = element.getAttribute('data-original-content');
        element.innerHTML = originalContent;
        element.disabled = false;
    }
}

// Data Table with Search and Pagination
class DataTable {
    constructor(tableElement, data, columns, options = {}) {
        this.table = tableElement;
        this.data = data;
        this.columns = columns;
        this.options = {
            searchable: true,
            sortable: true,
            pageSize: options.pageSize || 10,
            ...options
        };
        this.currentPage = 1;
        this.searchQuery = '';
        this.sortColumn = null;
        this.sortDirection = 'asc';
        
        this.render();
    }

    render() {
        const filteredData = this.getFilteredData();
        const paginatedData = this.getPaginatedData(filteredData);
        
        let html = '<div class="overflow-hidden">';
        
        // Search
        if (this.options.searchable) {
            html += `
                <div class="mb-4">
                    <div class="relative">
                        <input type="text" 
                               class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent" 
                               placeholder="Procurar..."
                               oninput="window.dataTableInstance.search(this.value)">
                        <i class="fas fa-search absolute left-3 top-3 text-gray-400"></i>
                    </div>
                </div>
            `;
        }
        
        // Table
        html += '<div class="overflow-x-auto"><table class="min-w-full divide-y divide-gray-200">';
        
        // Header
        html += '<thead class="bg-gray-50"><tr>';
        this.columns.forEach(col => {
            html += `<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">${col.header}</th>`;
        });
        html += '</tr></thead>';
        
        // Body
        html += '<tbody class="bg-white divide-y divide-gray-200">';
        paginatedData.forEach(row => {
            html += '<tr class="hover:bg-gray-50">';
            this.columns.forEach(col => {
                const value = col.render ? col.render(row) : row[col.field];
                html += `<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${value}</td>`;
            });
            html += '</tr>';
        });
        html += '</tbody></table></div>';
        
        // Pagination
        const totalPages = Math.ceil(filteredData.length / this.options.pageSize);
        if (totalPages > 1) {
            html += '<div class="flex items-center justify-between mt-4">';
            html += `<div class="text-sm text-gray-500">Mostrando ${(this.currentPage - 1) * this.options.pageSize + 1} a ${Math.min(this.currentPage * this.options.pageSize, filteredData.length)} de ${filteredData.length} resultados</div>`;
            html += '<div class="flex space-x-2">';
            
            for (let i = 1; i <= totalPages; i++) {
                const activeClass = i === this.currentPage ? 'bg-primary text-white' : 'bg-white text-gray-700 hover:bg-gray-50';
                html += `<button class="px-3 py-1 border rounded ${activeClass}" onclick="window.dataTableInstance.goToPage(${i})">${i}</button>`;
            }
            
            html += '</div></div>';
        }
        
        html += '</div>';
        
        this.table.innerHTML = html;
        window.dataTableInstance = this;
    }

    search(query) {
        this.searchQuery = query.toLowerCase();
        this.currentPage = 1;
        this.render();
    }

    goToPage(page) {
        this.currentPage = page;
        this.render();
    }

    getFilteredData() {
        if (!this.searchQuery) return this.data;
        
        return this.data.filter(row => {
            return this.columns.some(col => {
                const value = String(row[col.field] || '').toLowerCase();
                return value.includes(this.searchQuery);
            });
        });
    }

    getPaginatedData(data) {
        const start = (this.currentPage - 1) * this.options.pageSize;
        const end = start + this.options.pageSize;
        return data.slice(start, end);
    }
}

// Global API instance
window.api = new APIClient();
window.ToastManager = ToastManager;
window.ModalManager = ModalManager;
window.LoadingManager = LoadingManager;
window.DataTable = DataTable;
window.FormValidator = FormValidator;

console.log('Admin Panel JS loaded successfully ✨');
