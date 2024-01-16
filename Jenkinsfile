pipeline {
    agent any

    stages {
        stage('Download SCP:SL') {
            steps {
                sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta public-beta validate +quit'
                sh 'ln -s "\$HOME/scpsl/SCPSL_Data/Managed" ".scpsl_libs"'
            }
        }
        stage('Build') {
            parallel {
                stage('Plugin') {
                    steps {
                        sh 'msbuild SCPDiscordPlugin/SCPDiscordPlugin.csproj -restore -p:PostBuildEvent='
                    }
                }
                stage('Bot') {
                    stages {
                        stage('Linux') {
                            steps {
                                dir(path: 'SCPDiscordBot') {
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_Small -p:PublishSingleFile=true -p:PublishTrimmed=true -r linux-x64 -c Release --output ./small'
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_SC -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r linux-x64 -c Release --self-contained true --output ./sc'
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_R2R -p:PublishReadyToRun=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r linux-x64 -c Release --self-contained true --output ./r2r'
                                }
                            }
                        }
                        stage('Windows') {
                            steps {
                                dir(path: 'SCPDiscordBot') {
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_Small -p:PublishSingleFile=true -p:PublishTrimmed=true -r win-x64 -c Release --no-restore --output ./small_win'
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_SC -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r win-x64 -c Release --self-contained true --no-restore --output ./sc_win'
                                    sh 'dotnet publish -p:AssemblyName=SCPDiscordBot_R2R -p:PublishReadyToRun=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r win-x64 -c Release --self-contained true --output ./r2r_win'
                                }
                            }
                        }
                    }
                }
            }
        }
        stage('Package') {
            parallel {
                stage('Plugin') {
                    steps {
                        sh 'mkdir dependencies'
                        sh 'mv SCPDiscordPlugin/bin/SCPDiscord.dll ./'
                        sh 'mv SCPDiscordPlugin/bin/System.Memory.dll dependencies'
                        sh 'mv SCPDiscordPlugin/bin/Google.Protobuf.dll dependencies'
                        sh 'mv SCPDiscordPlugin/bin/Newtonsoft.Json.dll dependencies'
                    }
                }
                stage('Bot') {
                    steps {
                       sh 'mv SCPDiscordBot/small/SCPDiscordBot_Small ./'
                       sh 'mv SCPDiscordBot/sc/SCPDiscordBot_SC ./'
                       sh 'mv SCPDiscordBot/r2r/SCPDiscordBot_R2R ./'
                       sh 'mv SCPDiscordBot/small_win/SCPDiscordBot_Small.exe ./'
                       sh 'mv SCPDiscordBot/sc_win/SCPDiscordBot_SC.exe ./'
                       sh 'mv SCPDiscordBot/r2r_win/SCPDiscordBot_R2R.exe ./'
                    }
                }
            }
        }
        stage('Archive') {
            steps {
                sh 'zip -r dependencies.zip dependencies'
                archiveArtifacts(artifacts: 'dependencies.zip', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscord.dll', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Small', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Small.exe', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_SC', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_SC.exe', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_R2R', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_R2R.exe', onlyIfSuccessful: true)
            }
        }
    }
}
