pipeline {
    agent any

    stages {
        stage('Download SCP:SL') {
            steps {
                sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta public-beta validate +quit'
                sh 'ln -s "\$HOME/scpsl/SCPSL_Data/Managed" ".scpsl_libs"'
                sh 'cp -r "SCPDiscordBot" "AOT"'
                sh 'cp -r "SCPDiscordBot" "SMALL"'
                sh 'cp -r "SCPDiscordBot" "SC"'
                sh 'cp -r "SCPDiscordBot" "AOT_Win"'
                sh 'cp -r "SCPDiscordBot" "SMALL_Win"'
                sh 'mv    "SCPDiscordBot" "SC_Win"'
            }
        }
        stage('Build') {
            parallel {
                stage('Plugin') {
                    steps {
                        sh 'msbuild SCPDiscordPlugin/SCPDiscordPlugin.csproj -restore -p:PostBuildEvent='
                    }
                }
                stage('Bot - AOT') {
                    steps {
                        dir(path: 'AOT') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -p:PublishReadyToRun=true\\
                            -r linux-x64\\
                            -c Release\\
                            --self-contained true\\
                            --output ./out
                            '''
                        }
                    }
                }
                stage('Bot - Small') {
                    steps {
                        dir(path: 'SMALL') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -r linux-x64\\
                            -c Release\\
                            --self-contained false\\
                            --output ./out
                            '''
                        }
                    }
                }
                stage('Bot - Self Contained') {
                    steps {
                        dir(path: 'SC') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -r linux-x64\\
                            -c Release\\
                            --self-contained true\\
                            --output ./out
                            '''
                        }
                    }
                }
                stage('Bot - AOT (Windows)') {
                    steps {
                        dir(path: 'AOT_Win') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -p:PublishReadyToRun=true\\
                            -r win-x64\\
                            -c Release\\
                            --self-contained true\\
                            --output ./out
                            '''
                        }
                    }
                }
                stage('Bot - Small (Windows)') {
                    steps {
                        dir(path: 'SMALL_Win') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -r win-x64\\
                            -c Release\\
                            --self-contained false\\
                            --output ./out
                            '''
                        }
                    }
                }
                stage('Bot - Self Contained (Windows)') {
                    steps {
                        dir(path: 'SC_Win') {
                            sh '''dotnet publish\\
                            -p:PublishSingleFile=true\\
                            -r win-x64\\
                            -c Release\\
                            --self-contained true\\
                            --output ./out
                            '''
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
                       sh 'mv AOT/out/SCPDiscordBot ./SCPDiscordBot_Linux_AOT'
                       sh 'mv SMALL/out/SCPDiscordBot ./SCPDiscordBot_Linux'
                       sh 'mv SC/out/SCPDiscordBot ./SCPDiscordBot_Linux_SC'
                       sh 'mv AOT_Win/out/SCPDiscordBot.exe ./SCPDiscordBot_Windows_AOT.exe'
                       sh 'mv SMALL_Win/out/SCPDiscordBot.exe ./SCPDiscordBot_Windows.exe'
                       sh 'mv SC_Win/out/SCPDiscordBot.exe ./SCPDiscordBot_Windows_SC.exe'
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
                archiveArtifacts(artifacts: 'SCPDiscordBot_Linux_SC', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Windows_SC.exe', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Linux_AOT', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Windows_AOT.exe', onlyIfSuccessful: true)
            }
        }
    }
}
