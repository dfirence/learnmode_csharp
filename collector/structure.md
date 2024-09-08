/Collector
│
├── /src
│   ├── /core
│   │   ├── ICollector.cs               # Common interface for collectors
│   │   ├── IRegistryCollector.cs       # Interface for registry-specific collectors
│   │   ├── IPEFileCollector.cs         # Interface for PE file-specific collectors
│   │   └── IOutputFormatter.cs         # Interface for output formats (JSON, CSV, Parquet)
│   │
│   ├── /collectors
│   │   ├── RegistryCollector.cs        # Implements IRegistryCollector, enumerates registry entries
│   │   ├── PeFileCollector.cs          # Implements IPEFileCollector, collects and processes PE files
│   │
│   ├── /output
│   │   ├── JsonOutputFormatter.cs      # Implements IOutputFormatter, handles JSON serialization
│   │   ├── CsvOutputFormatter.cs       # Implements IOutputFormatter, handles CSV output
│   │   ├── ParquetOutputFormatter.cs   # Implements IOutputFormatter, handles Parquet output
│   │
│   ├── /utils
│   │   ├── FileUtils.cs                # Utilities for file handling (e.g., file checks, hashing)
│   │   ├── RegistryUtils.cs            # Utilities for registry access (e.g., error handling)
│   │   ├── ProgressBar.cs              # Utility for displaying progress
│   │   └── ParallelHelper.cs           # Helper functions for parallel processing
│   │
│   ├── Program.cs                      # Entry point, orchestrates collectors and formatters
│   └── AppSettings.json                # Configuration settings for the collector (e.g., paths, formats)
│
├── /tests
│   ├── /unit_tests
│   │   ├── RegistryCollectorTests.cs   # Unit tests for registry collection
│   │   ├── PeFileCollectorTests.cs     # Unit tests for PE file collection
│   │   └── OutputFormatterTests.cs     # Unit tests for JSON, CSV, and Parquet output
│   │
│   ├​⬤