# 🏭 Guía de Producción

Esta guía detalla cómo desplegar la aplicación en un entorno de producción.

## ✅ Checklist Previo a Producción

- [ ] Cambiar credenciales en `.env` (admin123 → contraseña fuerte)
- [ ] Configurar HTTPS/SSL
- [ ] Habilitar autenticación (JWT o Identity)
- [ ] Configurar CORS específicamente
- [ ] Configurar backup automático de BD
- [ ] Implementar rate limiting
- [ ] Configurar logging centralizado (ELK/Splunk)
- [ ] Monitoreo con Prometheus/Grafana
- [ ] Pruebas de carga
- [ ] Plan de recuperación ante desastres

---

## 🐳 Despliegue con Docker

### En Producción

1. **Clonar repositorio en servidor**
```bash
git clone https://github.com/usuario/Ambientes.git /opt/ambientes
cd /opt/ambientes
```

2. **Crear archivo `.env` con credenciales reales**
```bash
cp .env.example .env
nano .env  # Editar con contraseñas seguras
```

3. **Configurar para producción**

Editar `docker-compose.yml`:
```yaml
services:
  api:
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      Logging__LogLevel__Default: Warning
```

4. **Ejecutar con seguridad**
```bash
# Usar systemd para auto-reinicio
sudo systemctl enable docker

# Lanzar con logs
docker-compose up -d
docker-compose logs -f
```

### Backup y Restauración

```bash
# Backup de BD
docker exec ambientes-postgres pg_dump -U admin -d ambientes_db > backup.sql

# Restaurar
docker exec -i ambientes-postgres psql -U admin -d ambientes_db < backup.sql
```

---

## ☁️ Despliegue en Azure Container Instances

```bash
# Crear resource group
az group create --name ambientes-rg --location eastus

# Desplegar
az container create \
  --resource-group ambientes-rg \
  --name ambientes-api \
  --image ambientes-api:latest \
  --ports 8080 \
  --environment-variables \
    ConnectionStrings__PostgreSQL="Host=db-host;..." \
    ASPNETCORE_ENVIRONMENT="Production"
```

---

## 🚀 Despliegue en AWS ECS

1. Crear cluster ECS
2. Crear task definition con imagen Docker
3. Crear servicio con load balancer
4. RDS para PostgreSQL

---

## 🛡️ Configuración de Seguridad

### HTTPS/SSL
```yaml
# docker-compose.yml
api:
  volumes:
    - ./certs:/app/certs
  environment:
    ASPNETCORE_Kestrel__Certificates__Default__Path: /app/certs/cert.pfx
    ASPNETCORE_Kestrel__Certificates__Default__Password: password
```

### Autenticación JWT
Agregar en `Program.cs`:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://your-auth-server";
        options.Audience = "api";
    });
```

### Rate Limiting
```csharp
builder.Services.AddRateLimiting(configure =>
{
    configure.AddFixedWindowLimiter(
        policyName: "fixed",
        options => {
            options.PermitLimit = 10;
            options.Window = TimeSpan.FromSeconds(10);
        });
});
```

---

## 📊 Monitoreo

### Health Checks
```
GET http://api:8080/health
```

### Logs Centralizados

Configurar Serilog para Elasticsearch:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Elasticsearch(...)
    .CreateLogger();
```

### Métricas con Prometheus
```csharp
builder.Services.AddPrometheusActuator();
```

Acceder en: `http://api:8080/metrics`

---

## 🔄 CI/CD Pipeline (GitHub Actions)

`.github/workflows/docker-deploy.yml`:
```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker image
        run: docker build -t ambientes-api:${{ github.sha }} .
      
      - name: Push to registry
        run: docker push ambientes-api:${{ github.sha }}
      
      - name: Deploy to server
        run: |
          ssh user@server "cd /opt/ambientes && docker-compose pull && docker-compose up -d"
```

---

## 📈 Escalabilidad

### Load Balancer
```yaml
services:
  nginx:
    image: nginx:latest
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api1
      - api2
  
  api1:
    # ...
  
  api2:
    # ...
```

### Réplicas de BD
- Usar streaming replication de PostgreSQL
- Master-master setup para failover

---

## 💾 Backup y Recuperación

### Backup Automático
```bash
#!/bin/bash
# backup.sh
BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)

docker exec ambientes-postgres pg_dump -U admin ambientes_db > \
  $BACKUP_DIR/backup_$DATE.sql

# Mantener solo últimos 7 días
find $BACKUP_DIR -mtime +7 -delete
```

Agregar a crontab:
```bash
0 2 * * * /path/to/backup.sh
```

---

## 🆘 Troubleshooting en Producción

### API no responde
```bash
# Ver logs
docker-compose logs api | tail -100

# Reiniciar
docker-compose restart api

# Verificar recursos
docker stats
```

### BD Corrupta
```bash
# Conectar y reparar
docker exec -it ambientes-postgres psql -U admin -c "REINDEX DATABASE ambientes_db;"
```

### Bajo rendimiento
```sql
-- Analizar query
EXPLAIN ANALYZE SELECT * FROM ambientes WHERE estado = 'Disponible';

-- Reindex
REINDEX TABLE ambientes;

-- Vacuum
VACUUM ANALYZE ambientes;
```

---

**Mantener la seguridad y confiabilidad en todo momento 🛡️**
