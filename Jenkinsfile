pipeline {
  agent {
    label "jenkins-dotnet"
  }
  environment {
    ORG = 'lunmatu101'
    APP_NAME = 'jenkinx-microservice-netcore'
    CHARTMUSEUM_CREDS = credentials('jenkins-x-chartmuseum')
    DOCKER_REGISTRY_ORG = 'lunmatu101'
  }
  stages {
    stage('CI Build and push snapshot') {
      when {
        branch 'PR-*'
      }
      environment {
        PREVIEW_VERSION = "0.0.0-SNAPSHOT-$BRANCH_NAME-$BUILD_NUMBER"
        PREVIEW_NAMESPACE = "$APP_NAME-$BRANCH_NAME".toLowerCase()
        HELM_RELEASE = "$PREVIEW_NAMESPACE".toLowerCase()
      }
      steps {
        container('dotnet') {
          sh "jx step credential -s npm-token -k file -f /builder/home/.npmrc --optional=true"
          
          dir('./src/MyLib.Tests') {
            sh 'dotnet tool install dotnet-reportgenerator-globaltool --tool-path /tools'

            sh 'echo "Executing TDD..."'
            sh 'dotnet test --filter Category=TDD -r ./TestResults -l "trx;LogFileName=./report.xml" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/coverage/"'
            sh 'echo "Passed TDD"'

            sh 'echo "Executing BDD..."'
            sh 'dotnet test --filter Category=BDD'
            sh 'echo "Passed BDD"'

            sh '/tools/reportgenerator "-reports:./TestResults/coverage/coverage.cobertura.xml" "-targetdir:./TestResults/reports" "-reporttypes:HTML"'
          }

          dir('./src/MyLib') {
            sh 'dotnet tool install --global dotnet-sonarscanner --version 4.6.2' 
            sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet-sonarscanner begin /k:"MyLib" /d:sonar.host.url="10.108.94.149" /d:sonar.login="789b711cb3833cad56ba9aba00a265f8b5faac4c"'
            sh 'dotnet build -c Release -o ./app'
            sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet-sonarscanner end /d:sonar.login="789b711cb3833cad56ba9aba00a265f8b5faac4c"'
          }

          sh "export VERSION=$PREVIEW_VERSION && skaffold build -f skaffold.yaml"
          sh "jx step post build --image $DOCKER_REGISTRY/$ORG/$APP_NAME:$PREVIEW_VERSION"
          dir('./charts/preview') {
            sh "make preview"
            sh "jx preview --app $APP_NAME --dir ../.."
          }
        }
      }
    }
    stage('Build Release') {
      when {
        branch 'master'
      }
      steps {
        container('dotnet') {

          // ensure we're not on a detached head
          sh "git checkout master"
          sh "git config --global credential.helper store"
          sh "jx step git credentials"

          // so we can retrieve the version in later steps
          sh "echo \$(jx-release-version) > VERSION"
          sh "jx step tag --version \$(cat VERSION)"
          sh "jx step credential -s npm-token -k file -f /builder/home/.npmrc --optional=true"

          sh "export VERSION=`cat VERSION` && skaffold build -f skaffold.yaml"
          sh "jx step post build --image $DOCKER_REGISTRY/$ORG/$APP_NAME:\$(cat VERSION)"
        }
      }
    }
    stage('Promote to Environments') {
      when {
        branch 'master'
      }
      steps {
        container('dotnet') {
          dir('./charts/jenkinx-microservice-netcore') {
            sh "jx step changelog --batch-mode --version v\$(cat ../../VERSION)"

            // release the helm chart
            sh "jx step helm release"

            // promote through all 'Auto' promotion Environments
            sh "jx promote -b --all-auto --timeout 1h --version \$(cat ../../VERSION)"
          }
        }
      }
    }
  }
  post {
        always {
          publishHTML target: [
            allowMissing: false,
            alwaysLinkToLastBuild: true,
            keepAll: true,
            reportDir: './src/MyLib.Tests/TestResults/reports',
            reportFiles: 'index.htm',
            reportName: 'Code Coverage Report'
          ]
          cleanWs()
        }
  }
}
