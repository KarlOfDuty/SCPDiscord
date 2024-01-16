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
                       sh 'mv small/SCPDiscordBot_Small ../'
                       sh 'mv sc/SCPDiscordBot_SC ../'
                       sh 'mv r2r/SCPDiscordBot_R2R ../'
                       sh 'mv small_win/SCPDiscordBot_Small.exe ../'
                       sh 'mv sc_win/SCPDiscordBot_SC.exe ../'
                       sh 'mv r2r_win/SCPDiscordBot_R2R.exe ../'
                    }
                }
            }
        }
        stage('Archive') {
            steps {
                sh 'zip -r dependencies.zip dependencies'
                archiveArtifacts(artifacts: 'dependencies.zip', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscord.dll', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Linux', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Windows.exe', onlyIfSuccessful: true)
            }
        }
    }
}
