/**
 * Validaciones globales para el sistema de gestión de llaves.
 * Bloquea caracteres especiales en campos de texto y letras en campos numéricos.
 */

document.addEventListener('DOMContentLoaded', () => {
    // Aplicar validaciones a elementos con atributos data-validation
    const inputs = document.querySelectorAll('input[data-validation], textarea[data-validation]');
    
    inputs.forEach(input => {
        const type = input.getAttribute('data-validation');
        
        input.addEventListener('keypress', (e) => {
            if (type === 'alphanumeric') {
                // Permitir: letras (inc. ñ y acentos), números, espacios, guiones
                const regex = /^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s-]$/;
                if (!regex.test(e.key)) {
                    e.preventDefault();
                }
            } else if (type === 'numeric') {
                // Permitir: solo números
                const regex = /^[0-9]$/;
                if (!regex.test(e.key)) {
                    e.preventDefault();
                }
            }
        });

        // También validar al pegar texto
        input.addEventListener('paste', (e) => {
            const pastedText = (e.clipboardData || window.clipboardData).getData('text');
            let regex;
            
            if (type === 'alphanumeric') {
                regex = /^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s-]*$/;
            } else if (type === 'numeric') {
                regex = /^[0-9]*$/;
            }

            if (regex && !regex.test(pastedText)) {
                e.preventDefault();
                alert('El texto pegado contiene caracteres no permitidos.');
            }
        });
    });
});
